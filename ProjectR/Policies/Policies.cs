using System;
using System.Linq.Expressions;

namespace ProjectR
{
    public enum MappingStrategy
    {
        UsePublicConstructors = 1,
        UseStaticFactories = 2,
        UsePublicSetters = 3,
    }

    // NUOVA INTERFACCIA DI CONFIGURAZIONE
    public interface IPolicyConfiguration
    {
        /// <summary>
        /// Configures the policy for the Project(TEntity) method.
        /// </summary>
        IProjectAsPolicyBuilder<TSource, TDestination> ForProjection<TSource, TDestination>();

        /// <summary>
        /// Configures the policy for the Build(TDto) method.
        /// </summary>
        IBuildPolicyBuilder<TSource, TDestination> ForCreation<TSource, TDestination>();

        /// <summary>
        /// Configures the policy for the Apply(TDto, TEntity) method.
        /// </summary>
        IApplyToPolicyBuilder<TSource, TDestination> ForModification<TSource, TDestination>();
    }

    // NUOVA INTERFACCIA DI BASE PER LE CONFIGURAZIONI A LIVELLO DI PROPRIETÀ
    public interface IMemberConfiguration<TSource, TDestination>
    {
        /// <summary>
        /// Specifies a custom mapping expression for the selected destination member.
        /// </summary>
        /// <param name="expression">A lambda expression that calculates the value.</param>
        void From(Expression<Func<TSource, object>> expression);

        /// <summary>
        /// Maps the destination member from a source member with a different name.
        /// </summary>
        /// <param name="sourceMember">A lambda expression selecting the source member.</param>
        void FromSource(Expression<Func<TSource, object>> sourceMember);
    }

    // NUOVA INTERFACCIA DI BASE PER TUTTI I BUILDER
    public interface IPolicyBuilder<TSource, TDestination>
    {
        /// <summary>
        /// Configures the mapping for a specific destination member.
        /// </summary>
        /// <param name="destinationMember">A lambda expression selecting the destination member.</param>
        /// <returns>A configuration object for the specified member.</returns>
        IMemberConfiguration<TSource, TDestination> Map(Expression<Func<TDestination, object>> destinationMember);

        /// <summary>
        /// Ignores a specific destination member during the mapping.
        /// </summary>
        /// <param name="destinationMember">A lambda expression selecting the destination member to ignore.</param>
        void Ignore(Expression<Func<TDestination, object>> destinationMember);
    }

    // INTERFACCE SPECIALIZZATE
    public interface IProjectAsPolicyBuilder<TSource, TDestination> : IPolicyBuilder<TSource, TDestination> { }

    public interface IBuildPolicyBuilder<TSource, TDestination> : IPolicyBuilder<TSource, TDestination>
    {
        /// <summary>
        /// Adds a standard strategy to the sequence of attempts.
        /// </summary>
        IBuildPolicyBuilder<TSource, TDestination> Try(MappingStrategy strategy);

        /// <summary>
        /// Configures the mapping for a specific constructor or factory method parameter.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>A configuration object for the specified parameter.</returns>
        IMemberConfiguration<TSource, TDestination> MapParameter(string parameterName);
    }

    public interface IApplyToPolicyBuilder<TSource, TDestination> : IPolicyBuilder<TSource, TDestination>
    {
        /// <summary>
        /// Adds a standard strategy to the policy chain.
        /// </summary>
        IApplyToPolicyBuilder<TSource, TDestination> Try(MappingStrategy strategy);

        /// <summary>
        /// Excludes the 'Id' property from the mapping by convention.
        /// </summary>
        IApplyToPolicyBuilder<TSource, TDestination> IgnoreId();
    }

    // Interfacce interne per la rappresentazione delle policy compilate (invariate)
    public interface ICreationPolicy
    {
        IReadOnlyList<MappingStrategy> Strategies { get; }
    }
    public interface IModificationPolicy
    {
        IReadOnlyList<MappingStrategy> Strategies { get; }
    }
}

