namespace ProjectR
{
    /// <summary>
    /// The core abstract class that developers will inherit from to define a mapper.
    /// Inheriting from this class is the trigger for the Source Generator.
    /// </summary>
    /// <typeparam name="TEntity">The entity type (e.g., the Domain Entity).</typeparam>
    /// <typeparam name="TDto">The dto type (e.g., the DTO).</typeparam>
    public abstract partial class Mapper<TEntity, TDto>
    {
        /// <summary>
        /// Projects a entity object into a new dto object.
        /// This implementation will be generated automatically.
        /// </summary>
        /// <param name="source">The entity object instance.</param>
        /// <returns>A new dto object instance.</returns>
        public abstract TDto Project(TEntity source);

        /// <summary>
        /// Builds a new entity object from a dto object.
        /// This implementation will be generated automatically based on creation policies.
        /// </summary>
        /// <param name="dto">The dto object instance.</param>
        /// <returns>A new entity object instance.</returns>
        public abstract TEntity Build(TDto dto);

        /// <summary>
        /// Applies the data from a dto object to an existing entity object.
        /// This implementation will be generated automatically based on modification policies.
        /// </summary>
        /// <param name="dto">The dto object containing the new data.</param>
        /// <param name="entityToUpdate">The existing entity object to update.</param>
        public abstract void Apply(TDto dto, TEntity entityToUpdate);

        /// <summary>
        /// A factory method used by the entity generator to create a new entity object.
        /// This method can be overridden to provide a custom creation logic.
        /// </summary>
        /// <param name="dto">The dto object instance.</param>
        /// <returns>A new entity object instance.</returns>
        public static TEntity BuildFactoryFallback(TDto dto) => default!;

        /// <summary>
        /// A refinement method used by the entity generator to apply final adjustments to the entity object after building.
        /// This method can be overridden to provide custom refinement logic.
        /// </summary>
        /// <param name="entity">The entity object instance created by the build process.</param>
        /// <param name="dto">The dto object instance.</param>
        /// <returns>The refined entity object instance.</returns>
        public virtual TEntity BuildRefiner(TEntity entity, TDto dto) => entity;

        /// <summary>
        /// A refinement method used by the entity generator to apply final adjustments to the dto object after projection.
        /// This method can be overridden to provide custom refinement logic.
        /// </summary>
        /// <param name="dto">The dto object instance created by the projection process.</param>
        /// <param name="entity">The entity object instance.</param>
        /// <returns>The refined dto object instance.</returns>
        public virtual TDto ProjectAsRefiner(TDto dto, TEntity entity) => dto;

        /// <summary>
        /// A refinement method used by the entity generator to apply final adjustments to the entity object after applying updates from a dto.
        /// This method can be overridden to provide custom refinement logic.
        /// </summary>
        /// <param name="entity">The entity object instance created by the build process.</param>
        /// <param name="dto">The dto object instance.</param>
        /// <returns>The refined entity object instance.</returns>
        public virtual void ApplyToRefiner(TEntity entity, TDto dto) { }
    }

    /// <summary>
    /// Provides extension methods for simplified mapping operations.
    /// </summary>
    public static class MapperExtensions
    {
        /// <summary>
        /// Applies the data from a dto object to an existing entity object using the specified mapper.
        /// This is a generic extension method that simplifies the process of updating a entity entity from a DTO.
        /// </summary>
        /// <typeparam name="TDto">The dto type (e.g., the DTO).</typeparam>
        /// <typeparam name="TEntity">The entity type (e.g., the Domain Entity).</typeparam>
        /// <typeparam name="TMapper">The specific mapper class inheriting from <see cref="Mapper{TSource, TDestination}"/>.</typeparam>
        /// <param name="dto">The dto object containing the new data.</param>
        /// <param name="entityToUpdate">The existing entity object to update.</param>
        /// <returns>The updated entity object instance.</returns>
        public static TEntity Apply<TDto, TEntity, TMapper>(this TDto dto, TEntity entityToUpdate)
            where TMapper : Mapper<TEntity, TDto>, new()
        {
            Mapper<TEntity, TDto> mapper = Activator.CreateInstance<TMapper>();
            mapper.Apply(dto, entityToUpdate);
            return entityToUpdate;
        }

        public static TDto Project<TDto, TEntity, TMapper>(this TEntity entity)
            where TMapper : Mapper<TEntity, TDto>, new()
        {
            Mapper<TEntity, TDto> mapper = Activator.CreateInstance<TMapper>();
            return mapper.Project(entity);
        }

        public static TEntity Build<TDto, TEntity, TMapper>(this TDto dto)
            where TMapper : Mapper<TEntity, TDto>, new()
        {
            Mapper<TEntity, TDto> mapper = Activator.CreateInstance<TMapper>();
            return mapper.Build(dto);
        }
    }
}