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
        public TDto Project(TEntity source)
        {
            return ProjectAsRefiner(ProjectGenerated(source), source);
        }

        /// <summary>
        /// Builds a new entity object from a dto object.
        /// This implementation will be generated automatically based on creation policies.
        /// </summary>
        /// <param name="dto">The dto object instance.</param>
        /// <returns>A new entity object instance.</returns>
        public TEntity Build(TDto dto)
        {
            return BuildRefiner(BuildGenerated(dto), dto);
        }

        /// <summary>
        /// A factory method used by the entity generator to create a new entity object.
        /// This method can be overridden to provide a custom creation logic.
        /// </summary>
        /// <param name="dto">The dto object instance.</param>
        /// <returns>A new entity object instance.</returns>
        public static TEntity BuildFactoryFallback(TDto dto) => default!;

        /// <summary>
        /// Projects the specified entity to a data transfer object (DTO) of type <typeparamref name="TDto"/>.
        /// </summary>
        /// <param name="source">The entity instance to project. Cannot be null.</param>
        /// <returns>A DTO of type <typeparamref name="TDto"/> representing the projected data from the source entity.</returns>
        public virtual TDto ProjectGenerated(TEntity source) => default!;

        /// <summary>
        /// Creates a new instance of <typeparamref name="TEntity"/> based on the data provided in the specified
        /// <typeparamref name="TDto"/> object.
        /// </summary>
        /// <param name="dto">The data transfer object containing the information used to construct the <typeparamref name="TEntity"/>
        /// instance. Cannot be null.</param>
        /// <returns>A new <typeparamref name="TEntity"/> instance populated with values from <paramref name="dto"/>.</returns>
        public virtual TEntity BuildGenerated(TDto dto) => default!;

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
    }

    /// <summary>
    /// Provides extension methods for simplified mapping operations.
    /// </summary>
    public static class MapperExtensions
    {

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