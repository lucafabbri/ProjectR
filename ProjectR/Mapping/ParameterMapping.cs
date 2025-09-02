namespace ProjectR;

/// <summary>
/// Represents the mapping of a source property to a constructor or method parameter.
/// </summary>
public class ParameterMapping
{
    /// <summary>
    /// Gets or sets the name of the property on the source object.
    /// </summary>
    public string SourcePropertyName { get; set; }

    /// <summary>
    /// Gets or sets the name of the parameter on the destination constructor or method.
    /// </summary>
    public string DestinationParameterName { get; set; }
}