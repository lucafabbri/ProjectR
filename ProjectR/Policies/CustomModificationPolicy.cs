using System.Collections.ObjectModel;

namespace ProjectR.Policies;

/// <summary>
/// Represents a custom modification policy built from a sequence of strategies.
/// </summary>
internal class CustomModificationPolicy : IModificationPolicy
{
    public IReadOnlyList<MappingStrategy> Strategies { get; }

    public CustomModificationPolicy(List<MappingStrategy> strategies)
    {
        Strategies = new ReadOnlyCollection<MappingStrategy>(strategies);
    }
}