namespace ProjectR.Services
{
    /// <summary>
    /// Defines a service for resolving mapper instances at runtime.
    /// </summary>
    public interface IMapperResolver
    {
        /// <summary>
        /// Retrieves a mapper instance for the specified entity and dto types.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <typeparam name="TDto">The dto type.</typeparam>
        /// <returns>An instance of the corresponding Mapper.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no mapper is registered for the given entity and dto types.
        /// </exception>
        Mapper<TEntity, TDto> GetMapper<TEntity, TDto>();
    }
}
