using System;

namespace ProjectR.Attributes
{
    /// <summary>
    /// Enables automated discovery of mappers in referenced assemblies.
    /// The Source Generator will scan referenced assemblies for generated registration methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class DiscoverMappersAttribute : Attribute
    {
    }
}
