using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace ProjectR
{
    internal class AnalysisHelper
    {
        private readonly Compilation _compilation;
        private readonly IReadOnlyList<INamedTypeSymbol> _availableMappers;

        public AnalysisHelper(Compilation compilation, IReadOnlyList<INamedTypeSymbol> availableMappers)
        {
            _compilation = compilation;
            _availableMappers = availableMappers;
        }

        public void FindBestConstructor(ITypeSymbol sourceType, ITypeSymbol destinationType, MappingPlan plan)
        {
            if (destinationType.IsRecord)
            {
                var primaryConstructor = FindRecordPrimaryConstructor(destinationType);
                if (primaryConstructor != null)
                {
                    FindBestCandidate(new[] { primaryConstructor }, sourceType, plan);
                }
                if (plan.Creation.Method is CreationMethod.None or CreationMethod.ParameterlessConstructor)
                {
                    plan.Creation.Method = CreationMethod.None;
                }
                return;
            }

            var allConstructors = destinationType.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Constructor && m.DeclaredAccessibility == Accessibility.Public).ToList();
            if (!allConstructors.Any())
            {
                plan.Creation.Method = CreationMethod.None;
                return;
            }

            FindBestCandidate(allConstructors, sourceType, plan);

            if (plan.Creation.Method == CreationMethod.None)
            {
                var parameterlessCtor = allConstructors.Any(c => c.Parameters.IsEmpty && c.DeclaredAccessibility == Accessibility.Public);
                plan.Creation.Method = parameterlessCtor ? CreationMethod.ParameterlessConstructor : CreationMethod.None;
            }
        }

        public void FindBestFactory(ITypeSymbol sourceType, ITypeSymbol destinationType, MappingPlan plan)
        {
            var factories = destinationType.GetMembers().OfType<IMethodSymbol>().Where(m => m.IsStatic && m.DeclaredAccessibility == Accessibility.Public && SymbolEqualityComparer.Default.Equals(m.ReturnType, destinationType));
            FindBestCandidate(factories, sourceType, plan, isFactory: true);
        }

        private void FindBestCandidate(IEnumerable<IMethodSymbol> candidates, ITypeSymbol sourceType, MappingPlan plan, bool isFactory = false)
        {
            var bestCandidate = candidates.Where(c => c.Parameters.Any()).OrderByDescending(m => m.Parameters.Length).FirstOrDefault(c => CanSatisfyParameters(c, sourceType, plan));
            if (bestCandidate != null)
            {
                plan.Creation.Method = isFactory ? CreationMethod.FactoryMethod : CreationMethod.ConstructorWithParameters;
                if (isFactory) plan.Creation.FactoryMethod = bestCandidate;
                else plan.Creation.Constructor = bestCandidate;
                plan.Creation.ParametersMap = MapMethodParameters(bestCandidate, sourceType, plan);
            }
        }

        public void MapRemainingProperties(ITypeSymbol sourceType, ITypeSymbol destinationType, MappingPlan plan, IEnumerable<string>? ignoredMembers)
        {
            var sourceProperties = sourceType.GetMembers().OfType<IPropertySymbol>().ToList();
            var alreadyMapped = new HashSet<string>(plan.Creation.ParametersMap.Values.Select(p => p.SourceProperty.Name).Concat(plan.Instructions.Select(i => i.Destination.Name)));
            var destinationProperties = destinationType.GetMembers().OfType<IPropertySymbol>().Where(p => !alreadyMapped.Contains(p.Name));

            var ignoredSet = ignoredMembers != null ? new HashSet<string>(ignoredMembers, System.StringComparer.OrdinalIgnoreCase) : new HashSet<string>();
            destinationProperties = destinationProperties.Where(p => !ignoredSet.Contains(p.Name));

            foreach (var destProp in destinationProperties)
            {
                // Find a matching source property (case-insensitive).
                var sourceProp = sourceProperties.FirstOrDefault(p => p.Name.ToLower() == destProp.Name.ToLower());
                if (sourceProp == null) continue;

                // Path 1: Direct assignment via a public setter.
                if (destProp.SetMethod != null && destProp.SetMethod.DeclaredAccessibility == Accessibility.Public && !destProp.SetMethod.IsInitOnly)
                {
                    if (SymbolEqualityComparer.Default.Equals(destProp.Type, sourceProp.Type))
                    {
                        plan.Instructions.Add(new SimplePropertyMapping(sourceProp, destProp));
                        continue;
                    }
                    var nestedMapper = FindMapperFor(sourceProp.Type, destProp.Type);
                    if (nestedMapper != null)
                    {
                        plan.Instructions.Add(new NestedPropertyMapping(sourceProp, nestedMapper, destProp));
                        continue;
                    }
                    if (IsCollection(sourceProp.Type) && IsCollection(destProp.Type))
                    {
                        var sourceElementType = GetCollectionElementType(sourceProp.Type);
                        var destElementType = GetCollectionElementType(destProp.Type);
                        if (sourceElementType != null && destElementType != null)
                        {
                            var elementMapper = FindMapperFor(sourceElementType, destElementType);
                            if (elementMapper != null)
                            {
                                plan.Instructions.Add(new CollectionPropertyMapping(sourceProp, elementMapper, destProp));
                                continue;
                            }
                        }
                    }
                }
            }
        }

        private bool CanSatisfyParameters(IMethodSymbol method, ITypeSymbol sourceType, MappingPlan plan)
        {
            var sourceProperties = sourceType.GetMembers().OfType<IPropertySymbol>().ToList();
            return method.Parameters.All(p =>
            {
                if (p.IsOptional || p.NullableAnnotation == NullableAnnotation.Annotated || p.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T) return true;
                if (plan.Creation.CustomParameterExpressions.ContainsKey(p.Name)) return true;

                var matchingProp = sourceProperties.FirstOrDefault(sp => string.Equals(sp.Name, p.Name, System.StringComparison.OrdinalIgnoreCase));
                if (matchingProp == null) return false;
                if (SymbolEqualityComparer.Default.Equals(matchingProp.Type, p.Type)) return true;
                return FindMapperFor(matchingProp.Type, p.Type) != null;
            });
        }

        private Dictionary<IParameterSymbol, CreationInfo.ParameterMappingInfo> MapMethodParameters(IMethodSymbol method, ITypeSymbol sourceType, MappingPlan plan)
        {
            var sourceProperties = sourceType.GetMembers().OfType<IPropertySymbol>().ToList();
            var map = new Dictionary<IParameterSymbol, CreationInfo.ParameterMappingInfo>(SymbolEqualityComparer.Default);
            foreach (var param in method.Parameters)
            {
                if (plan.Creation.CustomParameterExpressions.ContainsKey(param.Name)) continue;
                var sourceProp = sourceProperties.FirstOrDefault(sp => string.Equals(sp.Name, param.Name, System.StringComparison.OrdinalIgnoreCase));
                if (sourceProp != null)
                {
                    INamedTypeSymbol? mapper = null;
                    if (!SymbolEqualityComparer.Default.Equals(sourceProp.Type, param.Type))
                    {
                        mapper = FindMapperFor(sourceProp.Type, param.Type);
                    }
                    map[param] = new CreationInfo.ParameterMappingInfo(sourceProp, mapper);
                }
            }
            return map;
        }

        private IMethodSymbol? FindRecordPrimaryConstructor(ITypeSymbol recordType)
        {
            var recordProperties = recordType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic).ToDictionary(p => p.Name, System.StringComparer.OrdinalIgnoreCase);
            if (!recordProperties.Any()) return null;
            return recordType.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(m => m.MethodKind == MethodKind.Constructor);
        }

        private INamedTypeSymbol? FindMapperFor(ITypeSymbol sourceType, ITypeSymbol destinationType)
        {
            var mapperBase = _compilation.GetTypeByMetadataName("ProjectR.Mapper`2");
            if (mapperBase == null) return null;
            return _availableMappers.FirstOrDefault(mapper =>
            {
                var baseType = mapper.BaseType;
                if (baseType == null || !SymbolEqualityComparer.Default.Equals(baseType.OriginalDefinition, mapperBase)) return false;
                if (SymbolEqualityComparer.Default.Equals(baseType.TypeArguments[0], sourceType) && SymbolEqualityComparer.Default.Equals(baseType.TypeArguments[1], destinationType)) return true;
                if (SymbolEqualityComparer.Default.Equals(baseType.TypeArguments[0], destinationType) && SymbolEqualityComparer.Default.Equals(baseType.TypeArguments[1], sourceType)) return true;
                return false;
            });
        }

        private bool IsCollection(ITypeSymbol type)
        {
            var ienumerable = _compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
            if (ienumerable == null) return false;
            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, ienumerable)) return true;
            return type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, ienumerable));
        }

        private ITypeSymbol? GetCollectionElementType(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol ntSymbol && ntSymbol.IsGenericType) return ntSymbol.TypeArguments.FirstOrDefault();
            var ienumerableInterface = type.AllInterfaces.FirstOrDefault(i => i.Name == "IEnumerable" && i.IsGenericType);
            return ienumerableInterface?.TypeArguments.FirstOrDefault();
        }
    }
}

