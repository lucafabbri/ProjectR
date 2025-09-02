namespace ProjectR.Policies;

/// <summary>
/// Provides methods to build a creation policy by chaining strategies.
/// </summary>
public interface ICreationPolicyBuilder
{
    /// <summary>
    /// Adds a strategy to the sequence of attempts. The strategies will be tried in the order they are added.
    /// </summary>
    /// <param name="strategy">The mapping strategy to add to the sequence.</param>
    /// <returns>The same builder instance for fluent chaining.</returns>
    ICreationPolicyBuilder Try(MappingStrategy strategy);
}
