namespace ProjectR.Services
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Implements the mapper resolution logic by leveraging the DI container.
    /// </summary>
    public sealed class MapperResolver : IMapperResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public MapperResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public Mapper<TSource, TDestination> GetMapper<TSource, TDestination>()
        {
            return _serviceProvider.GetRequiredService<Mapper<TSource, TDestination>>();
        }
    }
}
