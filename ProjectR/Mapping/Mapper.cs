namespace ProjectR
{
    /// <summary>
    /// The core abstract class that developers will inherit from to define a mapper.
    /// Inheriting from this class is the trigger for the Source Generator.
    /// </summary>
    /// <typeparam name="TSource">The source type (e.g., the Domain Entity).</typeparam>
    /// <typeparam name="TDestination">The destination type (e.g., the DTO).</typeparam>
    public abstract partial class Mapper<TSource, TDestination>
    {
        /// <summary>
        /// Projects a source object into a new destination object.
        /// This implementation will be generated automatically.
        /// </summary>
        /// <param name="source">The source object instance.</param>
        /// <returns>A new destination object instance.</returns>
        public abstract TDestination ProjectAs(TSource source);

        /// <summary>
        /// Builds a new source object from a destination object.
        /// This implementation will be generated automatically based on creation policies.
        /// </summary>
        /// <param name="destination">The destination object instance.</param>
        /// <returns>A new source object instance.</returns>
        public abstract TSource Build(TDestination destination);

        /// <summary>
        /// Applies the data from a destination object to an existing source object.
        /// This implementation will be generated automatically based on modification policies.
        /// </summary>
        /// <param name="destination">The destination object containing the new data.</param>
        /// <param name="sourceToUpdate">The existing source object to update.</param>
        public abstract void ApplyTo(TDestination destination, TSource sourceToUpdate);

        /// <summary>
        /// A factory method used by the source generator to create a new source object.
        /// This method can be overridden to provide a custom creation logic.
        /// </summary>
        /// <param name="destination">The destination object instance.</param>
        /// <returns>A new source object instance.</returns>
        public static TSource BuildFactoryFallback(TDestination destination) => default!;

        /// <summary>
        /// A refinement method used by the source generator to apply final adjustments to the source object after building.
        /// This method can be overridden to provide custom refinement logic.
        /// </summary>
        /// <param name="source">The source object instance created by the build process.</param>
        /// <param name="destination">The destination object instance.</param>
        /// <returns>The refined source object instance.</returns>
        public virtual TSource BuildRefiner(TSource source, TDestination destination) => source;

        /// <summary>
        /// A refinement method used by the source generator to apply final adjustments to the destination object after projection.
        /// This method can be overridden to provide custom refinement logic.
        /// </summary>
        /// <param name="destination">The destination object instance created by the projection process.</param>
        /// <param name="source">The source object instance.</param>
        /// <returns>The refined destination object instance.</returns>
        public virtual TDestination ProjectAsRefiner(TDestination destination, TSource source) => destination;
    }

    /// <summary>
    /// Provides extension methods for simplified mapping operations.
    /// </summary>
    public static class MapperExtensions
    {
        /// <summary>
        /// Applies the data from a destination object to an existing source object using the specified mapper.
        /// This is a generic extension method that simplifies the process of updating a source entity from a DTO.
        /// </summary>
        /// <typeparam name="TDestination">The destination type (e.g., the DTO).</typeparam>
        /// <typeparam name="TSource">The source type (e.g., the Domain Entity).</typeparam>
        /// <typeparam name="TMapper">The specific mapper class inheriting from <see cref="Mapper{TSource, TDestination}"/>.</typeparam>
        /// <param name="destination">The destination object containing the new data.</param>
        /// <param name="sourceToUpdate">The existing source object to update.</param>
        /// <returns>The updated source object instance.</returns>
        public static TSource ApplyTo<TDestination, TSource, TMapper>(this TDestination destination, TSource sourceToUpdate)
            where TMapper : Mapper<TSource, TDestination>, new()
        {
            Mapper<TSource, TDestination> mapper = Activator.CreateInstance<TMapper>();
            mapper.ApplyTo(destination, sourceToUpdate);
            return sourceToUpdate;
        }

        public static TDestination ProjectAs<TDestination, TSource, TMapper>(this TSource source)
            where TMapper : Mapper<TSource, TDestination>, new()
        {
            Mapper<TSource, TDestination> mapper = Activator.CreateInstance<TMapper>();
            return mapper.ProjectAs(source);
        }

        public static TSource Build<TDestination, TSource, TMapper>(this TDestination destination)
            where TMapper : Mapper<TSource, TDestination>, new()
        {
            Mapper<TSource, TDestination> mapper = Activator.CreateInstance<TMapper>();
            return mapper.Build(destination);
        }
    }
}