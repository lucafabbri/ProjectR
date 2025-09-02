using Microsoft.CodeAnalysis;

namespace ProjectR;

/// <summary>
/// Istruzione per mappare una o più proprietà sorgente a una proprietà di destinazione.
/// </summary>
public abstract class PropertyMappingInstruction : MappingInstruction
{
    public IPropertySymbol DestinationProperty { get; }
    protected PropertyMappingInstruction(IPropertySymbol destinationProperty) => DestinationProperty = destinationProperty;
}
