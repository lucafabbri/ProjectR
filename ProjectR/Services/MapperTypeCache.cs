namespace ProjectR.Services
{
    /// <summary>
    /// A cache that holds a lookup from (sourceType, destinationType) pairs to the concrete mapper Type.
    /// This is built once at startup and registered as a singleton.
    /// </summary>
    internal sealed class MapperTypeCache
    {
        /// <summary>
        /// Gets the lookup dictionary for mapper types.
        /// </summary>
        public IReadOnlyDictionary<(Type, Type), Type> MapperTypes { get; }

        public MapperTypeCache(IEnumerable<Type> foundMappers)
        {
            var mapperDictionary = new Dictionary<(Type, Type), Type>();
            var mapperBaseType = typeof(Mapper<,>);

            foreach (var mapperType in foundMappers)
            {
                var baseType = mapperType.BaseType;
                if (baseType != null && baseType.IsGenericType && baseType.GetGenericTypeDefinition() == mapperBaseType)
                {
                    var genericArgs = baseType.GetGenericArguments();
                    var sourceType = genericArgs[0];
                    var destinationType = genericArgs[1];

                    // Register mapping in both directions for flexibility
                    mapperDictionary[(sourceType, destinationType)] = mapperType;
                    mapperDictionary[(destinationType, sourceType)] = mapperType;
                }
            }
            MapperTypes = mapperDictionary;
        }
    }
}
