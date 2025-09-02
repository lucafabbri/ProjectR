using System.Collections.ObjectModel;

namespace ProjectR;

/// <summary>
/// Represents a custom creation policy built from a sequence of strategies.
/// This object holds the result of a user's configuration.
/// </summary>
internal class CustomCreationPolicy : ICreationPolicy
{
    public IReadOnlyList<MappingStrategy> Strategies { get; }

    public CustomCreationPolicy(List<MappingStrategy> strategies)
    {
        Strategies = new ReadOnlyCollection<MappingStrategy>(strategies);
    }
}