using Microsoft.CodeAnalysis;

namespace ProjectR;

/// <summary>
/// Represents mapping a complex object property using another mapper (e.g., Address to AddressDto).
/// </summary>
public class NestedObjectMapperMapping : MappingInstruction
{
    public IPropertySymbol SourceProperty { get; set; } = default!;
    public IPropertySymbol DestinationProperty { get; set; } = default!;
    public INamedTypeSymbol MapperType { get; set; } = default!; // The mapper to use (e.g., AddressMapper)
}
