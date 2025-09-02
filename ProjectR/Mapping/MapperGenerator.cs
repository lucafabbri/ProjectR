using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

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
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is ClassDeclarationSyntax,
                    transform: static (ctx, _) => GetMapperClass(ctx))
                .Where(static c => c is not null);

            var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndClasses,
                static (spc, source) => Execute(source.Left, source.Right, spc));
        }

        private static ClassDeclarationSyntax? GetMapperClass(GeneratorSyntaxContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol namedTypeSymbol)
                return null;

            var baseType = namedTypeSymbol.BaseType;
            if (baseType == null || !baseType.IsGenericType)
                return null;

            if (baseType.Name == "Mapper" && baseType.ContainingNamespace.ToDisplayString() == "ProjectR")
            {
                return classDeclaration;
            }

            return null;
        }

        private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax?> classes, SourceProductionContext context)
        {
            if (classes.IsDefaultOrEmpty) return;

            var distinctClasses = classes.Where(c => c is not null).Distinct().Select(c => c!).ToList();
            var allMappers = new List<INamedTypeSymbol>();
            foreach (var mapperClass in distinctClasses)
            {
                var semanticModel = compilation.GetSemanticModel(mapperClass.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(mapperClass) is INamedTypeSymbol symbol)
                {
                    allMappers.Add(symbol);
                }
            }

            foreach (var mapperClass in distinctClasses)
            {
                try
                {
                    ProcessMapperClass(compilation, mapperClass, context, allMappers);
                }
                catch (Exception ex)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.UnexpectedError, mapperClass.GetLocation(), ex.Message, ex.StackTrace));
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

            // The generator's main job is to find the policy method (if it exists)
            // and pass it to the unified PolicyEngine.
            var policyMethodSyntax = mapperSymbol.GetMembers("ConfigureMappingPolicies")
                .FirstOrDefault(m => m.IsStatic && !m.IsImplicitlyDeclared)?
                .DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;

            var engine = new PolicyEngine(compilation, policyMethodSyntax, allMappers);

            var projectAsPlan = engine.CreateProjectAsPlan(sourceType, destinationType);
            var buildPlan = engine.CreateBuildPlan(destinationType, sourceType);
            var applyToPlan = engine.CreateApplyToPlan(destinationType, sourceType);

            // Report all diagnostics gathered during plan creation.
            projectAsPlan.Diagnostics.ForEach(context.ReportDiagnostic);
            buildPlan.Diagnostics.ForEach(context.ReportDiagnostic);
            applyToPlan.Diagnostics.ForEach(context.ReportDiagnostic);

            // If the Build plan is invalid, stop generation for this mapper.
            if (buildPlan.Creation.Method == CreationMethod.None)
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.NoValidCreationMethod, mapperClass.Identifier.GetLocation(), buildPlan.DestinationType.Name));
                return;
            }

            var codeBuilder = new CodeBuilder();
            var sourceCode = codeBuilder.BuildSource(mapperSymbol, projectAsPlan, buildPlan, applyToPlan);

            context.AddSource($"{mapperSymbol.Name}.g.cs", sourceCode);
        }
    }
}

