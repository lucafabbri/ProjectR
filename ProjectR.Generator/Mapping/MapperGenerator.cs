using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace ProjectR
{
    [Generator]
    public class MapperGenerator : IIncrementalGenerator
    {
        public MapperGenerator()
        {
            //#if DEBUG
            //            if (!Debugger.IsAttached)
            //            {
            //                Debugger.Launch();
            //            }
            //#endif
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var compilationAndClasses = context.CompilationProvider
                .Select((compilation, ct) =>
                {
                    var explicitMappers = new Dictionary<string, ClassDeclarationSyntax>();
                    var dtosWithAttribute = new List<(INamedTypeSymbol DtoSymbol, INamedTypeSymbol EntitySymbol)>();

                    var mapperBaseSymbol = compilation.GetTypeByMetadataName("ProjectR.Mapper`2");
                    var dtoAttributeSymbol = compilation.GetTypeByMetadataName("ProjectR.Attributes.DtoAttribute`1");
                    var excludeAttributeSymbol = compilation.GetTypeByMetadataName("ProjectR.Attributes.GeneratorExcludeAttribute");


                    // First pass: Find all explicit mappers and DTOs with attributes
                    foreach (var tree in compilation.SyntaxTrees)
                    {
                        var semanticModel = compilation.GetSemanticModel(tree);
                        foreach (var classNode in tree.GetRoot(ct).DescendantNodes().OfType<ClassDeclarationSyntax>())
                        {
                            if (semanticModel.GetDeclaredSymbol(classNode, ct) is not INamedTypeSymbol classSymbol) continue;

                            // Check for explicit mapper
                            if (mapperBaseSymbol != null && classSymbol.BaseType is { IsGenericType: true } &&
                                SymbolEqualityComparer.Default.Equals(classSymbol.BaseType.OriginalDefinition, mapperBaseSymbol))
                            {
                                bool hasExcludeAttribute = classSymbol.GetAttributes().Any(ad =>
                                    excludeAttributeSymbol != null &&
                                    ad.AttributeClass?.Equals(excludeAttributeSymbol, SymbolEqualityComparer.Default) == true);

                                if (!hasExcludeAttribute)
                                {
                                    explicitMappers[classSymbol.Name] = classNode;
                                }
                            }
                            
                            var attribute = classSymbol.GetAttributes().FirstOrDefault(ad =>
                                dtoAttributeSymbol != null && ad.AttributeClass?.OriginalDefinition.Equals(dtoAttributeSymbol, SymbolEqualityComparer.Default) == true);

                            if (attribute?.AttributeClass is { TypeArguments.Length: 1 } &&
                                attribute.AttributeClass.TypeArguments[0] is INamedTypeSymbol entitySymbol)
                            {
                                dtosWithAttribute.Add((classSymbol, entitySymbol));
                            }
                        }
                    }

                    return (compilation, explicitMappers, dtosWithAttribute);
                });

            context.RegisterSourceOutput(compilationAndClasses,
                static (spc, source) => Execute(source.compilation, source.explicitMappers, source.dtosWithAttribute, spc));
        }


        private static void Execute(Compilation compilation,
                                    Dictionary<string, ClassDeclarationSyntax> explicitMappers,
                                    List<(INamedTypeSymbol DtoSymbol, INamedTypeSymbol EntitySymbol)> dtosWithAttribute,
                                    SourceProductionContext context)
        {
            var allMapperClassesToImplement = new List<ClassDeclarationSyntax>(explicitMappers.Values);
            var placeholderSyntaxTrees = new List<SyntaxTree>();

            // Generate placeholders for DTOs that don't have an explicit mapper
            foreach (var (dtoSymbol, entitySymbol) in dtosWithAttribute)
            {
                var mapperName = $"{dtoSymbol.Name}Mapper";
                if (!explicitMappers.ContainsKey(mapperName))
                {
                    var entityFullName = entitySymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var dtoFullName = dtoSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var ns = dtoSymbol.ContainingNamespace.IsGlobalNamespace ? "" : $"namespace {dtoSymbol.ContainingNamespace.ToDisplayString()}";

                    var sourceText = $@"// <auto-generated-placeholder/>
{ns}
{{
    public partial class {mapperName} : global::ProjectR.Mapper<{entityFullName}, {dtoFullName}> {{ }}
}}";
                    context.AddSource($"{mapperName}.ph.g.cs", sourceText);
                    var parseOptions = (compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions) ?? CSharpParseOptions.Default;
                    var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, parseOptions, path: $"{mapperName}.ph.g.cs");
                    placeholderSyntaxTrees.Add(syntaxTree);

                    var placeholderClassNode = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
                    allMapperClassesToImplement.Add(placeholderClassNode);
                }
            }

            var compilationWithPlaceholders = compilation.AddSyntaxTrees(placeholderSyntaxTrees);
            var distinctClasses = allMapperClassesToImplement.Distinct().ToList();
            var allMappers = new List<INamedTypeSymbol>();

            foreach (var mapperClass in distinctClasses)
            {
                var semanticModel = compilationWithPlaceholders.GetSemanticModel(mapperClass.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(mapperClass) is INamedTypeSymbol symbol)
                {
                    allMappers.Add(symbol);
                }
            }

            foreach (var mapperClass in distinctClasses)
            {
                try
                {
                    ProcessMapperClass(compilationWithPlaceholders, mapperClass, context, allMappers);
                }
                catch (Exception ex)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.UnexpectedError, mapperClass.GetLocation(), ex.Message, ex.StackTrace));
                }
            }

            GenerateDiRegistration(compilation, allMappers, context);
        }

        private static void GenerateDiRegistration(Compilation compilation, List<INamedTypeSymbol> mappers, SourceProductionContext context)
        {
            var sb = new StringBuilder();
            var assemblyName = compilation.AssemblyName!;
            var safeAssemblyName = assemblyName.Replace(".", "_");
            var generatedNamespace = $"{assemblyName}.Generated";
            var extensionsClassName = $"GeneratedMapperRegistrations_{safeAssemblyName}";

            var discoverMappersAttributeSymbol = compilation.GetTypeByMetadataName("ProjectR.Attributes.DiscoverMappersAttribute");
            bool shouldDiscover = compilation.Assembly.GetAttributes().Any(ad => 
                discoverMappersAttributeSymbol != null && 
                ad.AttributeClass?.Equals(discoverMappersAttributeSymbol, SymbolEqualityComparer.Default) == true);

            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection.Extensions;");
            sb.AppendLine("using ProjectR.Services;");
            sb.AppendLine();

            var namespaces = mappers
                .Select(m => m.ContainingNamespace.ToDisplayString())
                .Distinct()
                .OrderBy(ns => ns);

            foreach (var ns in namespaces)
            {
                sb.AppendLine($"using {ns};");
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {generatedNamespace};");
            sb.AppendLine();
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// Generated mapper registration for {assemblyName}.");
            sb.AppendLine("/// Generated by MapperGenerator.");
            sb.AppendLine("/// </summary>");
            sb.AppendLine($"public static class {extensionsClassName}");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Registers all generated mappers in the DI container.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddGeneratedMappers(");
            sb.AppendLine("        this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)");
            sb.AppendLine("    {");

            // Register individual mappers
            sb.AppendLine("        // Register individual mappers from this assembly");
            foreach (var mapper in mappers.OrderBy(m => m.Name))
            {
                var fullyQualifiedName = mapper.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var baseType = mapper.BaseType;
                if (baseType != null && baseType.TypeArguments.Length == 2)
                {
                    var sourceType = baseType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var destinationType = baseType.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    
                    sb.AppendLine($"        services.TryAddSingleton<{fullyQualifiedName}>();");
                    sb.AppendLine($"        services.TryAddSingleton<global::ProjectR.Mapper<{sourceType}, {destinationType}>>(sp => sp.GetRequiredService<{fullyQualifiedName}>());");
                }
            }
            sb.AppendLine();

            if (shouldDiscover)
            {
                sb.AppendLine("        // Discover and register mappers from referenced assemblies");
                foreach (var referencedAssembly in compilation.SourceModule.ReferencedAssemblySymbols)
                {
                    var refSafeName = referencedAssembly.Name.Replace(".", "_");
                    var refNamespace = $"{referencedAssembly.Name}.Generated";
                    var refClassName = $"GeneratedMapperRegistrations_{refSafeName}";
                    
                    // Check if the class exists in the referenced assembly
                    var refType = compilation.GetTypeByMetadataName($"{refNamespace}.{refClassName}");
                    if (refType != null)
                    {
                        sb.AppendLine($"        global::{refNamespace}.{refClassName}.AddGeneratedMappers(services);");
                    }
                }
                sb.AppendLine();
            }

            // Register global services (resolver)
            sb.AppendLine("        // Register global services (only once)");
            sb.AppendLine("        services.TryAddSingleton<global::ProjectR.Services.IMapperResolver, global::ProjectR.Services.MapperResolver>();");
            sb.AppendLine();

            sb.AppendLine("        return services;");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource("MapperRegistration.g.cs", sb.ToString());
        }

        private static void CollectUsings(ITypeSymbol typeSymbol, HashSet<string> usings)
        {
            foreach (var syntaxRef in typeSymbol.DeclaringSyntaxReferences)
            {
                var syntaxNode = syntaxRef.GetSyntax();
                if (syntaxNode?.SyntaxTree?.GetRoot() is CompilationUnitSyntax root)
                {
                    foreach (var u in root.Usings)
                    {
                        usings.Add(u.ToString());
                    }
                }
            }
        }

        private static void ProcessMapperClass(Compilation compilation, ClassDeclarationSyntax mapperClass, SourceProductionContext context, IReadOnlyList<INamedTypeSymbol> allMappers)
        {
            var semanticModel = compilation.GetSemanticModel(mapperClass.SyntaxTree);
            var mapperSymbol = semanticModel.GetDeclaredSymbol(mapperClass) as INamedTypeSymbol;
            if (mapperSymbol == null) return;

            var baseType = mapperSymbol.BaseType;
            if (baseType == null || baseType.TypeArguments.Length != 2) return;

            var sourceType = baseType.TypeArguments[0];
            var destinationType = baseType.TypeArguments[1];

            var allUsings = new HashSet<string>();
            CollectUsings(mapperSymbol, allUsings);
            CollectUsings(sourceType, allUsings);
            CollectUsings(destinationType, allUsings);

            var policyMethodSyntax = mapperSymbol.GetMembers("ConfigureMappingPolicies")
                .FirstOrDefault(m => m.IsStatic && !m.IsImplicitlyDeclared)?
                .DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;

            // Use the provided compilation which includes placeholders
            var engine = new PolicyEngine(compilation, policyMethodSyntax, allMappers);

            var projectAsPlan = engine.CreateProjectAsPlan(sourceType, destinationType);
            var buildPlan = engine.CreateBuildPlan(destinationType, sourceType);
            var applyToPlan = engine.CreateApplyToPlan(destinationType, sourceType);

            projectAsPlan.Diagnostics.ForEach(context.ReportDiagnostic);
            buildPlan.Diagnostics.ForEach(context.ReportDiagnostic);
            applyToPlan.Diagnostics.ForEach(context.ReportDiagnostic);

            if (buildPlan.Creation.Method == CreationMethod.None)
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.NoValidCreationMethod, mapperClass.Identifier.GetLocation(), buildPlan.DestinationType.Name));
                return;
            }

            var codeBuilder = new CodeBuilder();
            var sourceCode = codeBuilder.BuildSource(mapperSymbol, projectAsPlan, buildPlan, applyToPlan, allUsings);

            context.AddSource($"{mapperSymbol.Name}.g.cs", sourceCode);
        }
    }
}
