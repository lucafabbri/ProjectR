namespace ProjectR.Policies;

/// <summary>
/// Internal implementation of the modification policy builder.
/// </summary>
internal class ModificationPolicyBuilder : IModificationPolicyBuilder
{
    private readonly List<MappingStrategy> _strategies = new List<MappingStrategy>();

    public IModificationPolicyBuilder Try(MappingStrategy strategy)
    {
        _strategies.Add(strategy);
        return this;
    }

    internal IModificationPolicy BuildPolicy()
    {
        return new CustomModificationPolicy(_strategies);
    }
}
