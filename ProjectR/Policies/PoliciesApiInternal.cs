using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ProjectR
{
    /// <summary>
    /// Internal implementation of the policy configuration API contract.
    /// This is the entry point for the fluent configuration.
    /// </summary>
    internal class PolicyConfiguration : IPolicyConfiguration
    {
        public IProjectAsPolicyBuilder<TSource, TDestination> ForProjection<TSource, TDestination>()
        {
            return new ProjectAsPolicyBuilder<TSource, TDestination>();
        }

        public IBuildPolicyBuilder<TDestination, TSource> ForCreation<TDestination, TSource>()
        {
            return new BuildPolicyBuilder<TDestination, TSource>();
        }

        public IApplyToPolicyBuilder<TDestination, TSource> ForModification<TDestination, TSource>()
        {
            return new ApplyToPolicyBuilder<TDestination, TSource>();
        }
    }

    /// <summary>
    /// Internal implementation of the member configuration.
    /// It stores the specific mapping rule for a single property or parameter.
    /// </summary>
    internal class MemberConfiguration<TSource, TDestination> : IMemberConfiguration<TSource, TDestination>
    {
        public Expression<Func<TSource, object>>? FromExpression { get; private set; }
        public Expression<Func<TSource, object>>? FromSourceExpression { get; private set; }

        public void From(Expression<Func<TSource, object>> expression)
        {
            FromExpression = expression;
        }

        public void FromSource(Expression<Func<TSource, object>> sourceMember)
        {
            FromSourceExpression = sourceMember;
        }
    }

    /// <summary>
    /// A base class for policy builders to share common logic for member mapping and ignoring.
    /// </summary>
    internal abstract class PolicyBuilder<TSource, TDestination> : IPolicyBuilder<TSource, TDestination>
    {
        internal readonly Dictionary<string, MemberConfiguration<TSource, TDestination>> MemberConfigurations = new();
        internal readonly HashSet<string> IgnoredMembers = new();

        public IMemberConfiguration<TSource, TDestination> Map(Expression<Func<TDestination, object>> destinationMember)
        {
            var memberName = GetMemberName(destinationMember);
            var config = new MemberConfiguration<TSource, TDestination>();
            MemberConfigurations[memberName] = config;
            return config;
        }

        public void Ignore(Expression<Func<TDestination, object>> destinationMember)
        {
            var memberName = GetMemberName(destinationMember);
            IgnoredMembers.Add(memberName);
        }

        protected static string GetMemberName(Expression expression)
        {
            if (expression is LambdaExpression lambda)
                expression = lambda.Body;

            if (expression is MemberExpression member)
                return member.Member.Name;

            if (expression is UnaryExpression unary && unary.Operand is MemberExpression unaryMember)
                return unaryMember.Member.Name;

            throw new ArgumentException("Expression is not a valid member access expression.", nameof(expression));
        }
    }

    /// <summary>
    /// Internal implementation of the projection policy builder.
    /// </summary>
    internal class ProjectAsPolicyBuilder<TSource, TDestination>
        : PolicyBuilder<TSource, TDestination>, IProjectAsPolicyBuilder<TSource, TDestination>
    {
    }

    /// <summary>
    /// Internal implementation of the creation policy builder.
    /// </summary>
    internal class BuildPolicyBuilder<TSource, TDestination>
        : PolicyBuilder<TSource, TDestination>, IBuildPolicyBuilder<TSource, TDestination>
    {
        internal readonly List<MappingStrategy> Strategies = new();
        internal readonly Dictionary<string, MemberConfiguration<TSource, TDestination>> ParameterConfigurations = new();

        public IBuildPolicyBuilder<TSource, TDestination> Try(MappingStrategy strategy)
        {
            Strategies.Add(strategy);
            return this;
        }

        public IMemberConfiguration<TSource, TDestination> MapParameter(string parameterName)
        {
            var config = new MemberConfiguration<TSource, TDestination>();
            ParameterConfigurations[parameterName.ToLowerInvariant()] = config;
            return config;
        }
    }

    /// <summary>
    /// Internal implementation of the modification policy builder.
    /// </summary>
    internal class ApplyToPolicyBuilder<TSource, TDestination>
        : PolicyBuilder<TSource, TDestination>, IApplyToPolicyBuilder<TSource, TDestination>
    {
        internal readonly List<MappingStrategy> Strategies = new();

        public IApplyToPolicyBuilder<TSource, TDestination> Try(MappingStrategy strategy)
        {
            Strategies.Add(strategy);
            return this;
        }

        public IApplyToPolicyBuilder<TSource, TDestination> IgnoreId()
        {
            IgnoredMembers.Add("Id");
            return this;
        }
    }
}

