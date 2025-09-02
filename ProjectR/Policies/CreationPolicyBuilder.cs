namespace ProjectR.Policies;


/// <summary>
/// Internal implementation of the creation policy builder.
/// </summary>
internal class CreationPolicyBuilder : ICreationPolicyBuilder
{
    private readonly List<MappingStrategy> _strategies = new List<MappingStrategy>();

    public ICreationPolicyBuilder Try(MappingStrategy strategy)
    {
        _strategies.Add(strategy);
        return this;
    }

    internal ICreationPolicy BuildPolicy()
    {
        return new CustomCreationPolicy(_strategies);
    }
}
