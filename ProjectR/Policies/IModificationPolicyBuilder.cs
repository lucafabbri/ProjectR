namespace ProjectR.Policies;

/// <summary>
/// A builder for defining a sequence of strategies for object modification.
/// </summary>
public interface IModificationPolicyBuilder
{
    /// <summary>
    /// Adds a strategy to the policy chain.
    /// </summary>
    IModificationPolicyBuilder Try(MappingStrategy strategy);
}
