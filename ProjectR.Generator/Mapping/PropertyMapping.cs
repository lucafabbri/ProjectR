namespace ProjectR;

/// <summary>
/// Represents the direct mapping of a single property from a source to a destination.
/// </summary>
public class PropertyMapping : MappingInstruction
{
    /// <summary>
    /// Gets or sets the name of the property on the source object.
    /// </summary>
    public string SourcePropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the property on the destination object.
    /// </summary>
    public string DestinationPropertyName { get; set; } = string.Empty;
}
