using Microsoft.Extensions.DependencyInjection;
using ProjectR.Services;
using System.Reflection;

namespace ProjectR.DI
{
    /// <summary>
    /// Provides extension methods for registering mappers and the resolver service.
    /// </summary>
    public static class MapperRegistrationExtensions
    {
        /// <summary>
        /// Scans assemblies to discover and hold references to all concrete Mapper types.
        /// This class is designed to be used at application startup.
        /// </summary>
        public sealed class MapperRegistry
        {
            /// <summary>
            /// Gets the collection of discovered mapper types.
            /// </summary>
            public IReadOnlyCollection<Type> FoundMappers { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="MapperRegistry"/> class by scanning the provided assemblies.
            /// </summary>
            /// <param name="assembliesToScan">A collection of assemblies to scan for mapper implementations.</param>
            public MapperRegistry(IEnumerable<Assembly> assembliesToScan)
            {
                FoundMappers = ScanAssemblies(assembliesToScan);
            }

            private IReadOnlyCollection<Type> ScanAssemblies(IEnumerable<Assembly> assemblies)
            {
                var mapperBaseType = typeof(Mapper<,>);

                return assemblies
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type =>
                        type.IsClass &&
                        !type.IsAbstract &&
                        type.BaseType != null &&
                        type.BaseType.IsGenericType &&
                        type.BaseType.GetGenericTypeDefinition() == mapperBaseType)
                    .ToList()
                    .AsReadOnly();
            }
        }

        /// <summary>
        /// Scans the specified assemblies for concrete mapper implementations and registers them,
        /// along with the IMapperResolver service, in the dependency injection container.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <param name="assembliesToScan">The assemblies to scan for mappers.</param>
        /// <returns>The same IServiceCollection for chaining.</returns>
        public static IServiceCollection AddMappers(this IServiceCollection services, params Assembly[] assembliesToScan)
        {
            if (assembliesToScan == null || !assembliesToScan.Any())
            {
                //fill with Executing assembly
                assembliesToScan = [Assembly.GetCallingAssembly()];
            }

            var registry = new MapperRegistry(assembliesToScan);
            var cache = new MapperTypeCache(registry.FoundMappers);

            // Register the type cache as a singleton
            services.AddSingleton(cache);

            // Register each individual mapper as a singleton
            foreach (var mapperType in registry.FoundMappers)
            {
                services.AddSingleton(mapperType);
            }

            // Register the resolver service
            services.AddSingleton<IMapperResolver, MapperResolver>();

            return services;
        }
    }
}
