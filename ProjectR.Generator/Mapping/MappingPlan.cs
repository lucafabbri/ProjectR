using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace ProjectR
{
    public enum CreationMethod
    {
        None,
        ParameterlessConstructor,
        ConstructorWithParameters,
        FactoryMethod
    }

    public class CreationInfo
    {
        public class ParameterMappingInfo
        {
            public IPropertySymbol SourceProperty { get; }
            public INamedTypeSymbol? Mapper { get; }

            public ParameterMappingInfo(IPropertySymbol sourceProperty, INamedTypeSymbol? mapper)
            {
                SourceProperty = sourceProperty;
                Mapper = mapper;
            }
        }

        public CreationMethod Method { get; set; } = CreationMethod.None;
        public IMethodSymbol? Constructor { get; set; }
        public IMethodSymbol? FactoryMethod { get; set; }
        public Dictionary<IParameterSymbol, ParameterMappingInfo> ParametersMap { get; set; } = new(SymbolEqualityComparer.Default);
        public Dictionary<string, LambdaExpressionSyntax> CustomParameterExpressions { get; set; } = new();
    }

    public class MappingPlan
    {
        public ITypeSymbol SourceType { get; }
        public ITypeSymbol DestinationType { get; }
        public CreationInfo Creation { get; set; } = new CreationInfo();
        public List<IMappingInstruction> Instructions { get; } = new List<IMappingInstruction>();
        public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

        public MappingPlan(ITypeSymbol sourceType, ITypeSymbol destinationType)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
        }
    }

    public interface IMappingInstruction
    {
        IPropertySymbol Destination { get; }
    }

    public class CustomExpressionMapping : IMappingInstruction
    {
        public LambdaExpressionSyntax SourceExpression { get; }
        public IPropertySymbol Destination { get; }
        public CustomExpressionMapping(LambdaExpressionSyntax sourceExpression, IPropertySymbol destination)
        {
            SourceExpression = sourceExpression;
            Destination = destination;
        }
    }

    /// <summary>
    /// Represents an update via a method call that can take multiple source properties.
    /// e.g., destination.UpdateAddress(source.Street, source.City)
    /// </summary>
    public class MethodCallMapping : IMappingInstruction
    {
        public IReadOnlyList<IPropertySymbol> Sources { get; }
        public IMethodSymbol UpdateMethod { get; }
        public IPropertySymbol Destination { get; } // The conceptual property being updated.
        public MethodCallMapping(IReadOnlyList<IPropertySymbol> sources, IMethodSymbol updateMethod, IPropertySymbol destination)
        {
            Sources = sources;
            UpdateMethod = updateMethod;
            Destination = destination;
        }
    }

    public class SimplePropertyMapping : IMappingInstruction
    {
        public IPropertySymbol Source { get; }
        public IPropertySymbol Destination { get; }
        public SimplePropertyMapping(IPropertySymbol source, IPropertySymbol destination)
        {
            Source = source;
            Destination = destination;
        }
    }

    public class CompositePropertyMapping : IMappingInstruction
    {
        public IReadOnlyList<IPropertySymbol> Sources { get; }
        public string Format { get; }
        public IPropertySymbol Destination { get; }
        public CompositePropertyMapping(IReadOnlyList<IPropertySymbol> sources, string format, IPropertySymbol destination)
        {
            Sources = sources;
            Format = format;
            Destination = destination;
        }
    }

    public class NestedPropertyMapping : IMappingInstruction
    {
        public IPropertySymbol Source { get; }
        public INamedTypeSymbol Mapper { get; }
        public IPropertySymbol Destination { get; }
        public NestedPropertyMapping(IPropertySymbol source, INamedTypeSymbol mapper, IPropertySymbol destination)
        {
            Source = source;
            Mapper = mapper;
            Destination = destination;
        }
    }

    public class CollectionPropertyMapping : IMappingInstruction
    {
        public IPropertySymbol Source { get; }
        public INamedTypeSymbol ElementMapper { get; }
        public IPropertySymbol Destination { get; }
        public CollectionPropertyMapping(IPropertySymbol source, INamedTypeSymbol elementMapper, IPropertySymbol destination)
        {
            Source = source;
            ElementMapper = elementMapper;
            Destination = destination;
        }
    }
}
