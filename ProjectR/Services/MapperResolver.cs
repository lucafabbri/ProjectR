namespace ProjectR.Services
{
    /// <summary>
    /// Implements the mapper resolution logic by leveraging the DI container and a type cache.
    /// </summary>
    internal sealed class MapperResolver : IMapperResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MapperTypeCache _mapperTypeCache;

        public MapperResolver(IServiceProvider serviceProvider, MapperTypeCache mapperTypeCache)
        {
            _serviceProvider = serviceProvider;
            _mapperTypeCache = mapperTypeCache;
        }

        /// <inheritdoc/>
        public Mapper<TSource, TDestination> GetMapper<TSource, TDestination>()
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);

            if (!_mapperTypeCache.MapperTypes.TryGetValue((sourceType, destinationType), out var mapperType))
            {
                throw new InvalidOperationException($"No mapper registered for source '{sourceType.Name}' and destination '{destinationType.Name}'.");
            }

            // The concrete mapper type is registered as a singleton, so this retrieves the instance.
            var mapperInstance = _serviceProvider.GetService(mapperType);

            return (Mapper<TSource, TDestination>)mapperInstance;
        }
    }
}
