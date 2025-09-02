using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace ProjectR
{
    /// <summary>
    /// The single, unified engine responsible for creating a mapping plan.
    /// It either parses a user-defined policy or applies a default "zero-config" policy.
    /// </summary>
    internal class PolicyEngine
    {
        private readonly Compilation _compilation;
        private readonly MethodDeclarationSyntax? _policyMethod;
        private readonly SemanticModel? _semanticModel;
        private readonly IReadOnlyList<INamedTypeSymbol> _availableMappers;
        private readonly AnalysisHelper _analysisHelper;

        private class ParsedPolicy
        {
            public List<MappingStrategy> Strategies { get; set; } = new();
            public Dictionary<string, LambdaExpressionSyntax> MemberMappings { get; set; } = new();
            public Dictionary<string, LambdaExpressionSyntax> ParameterMappings { get; set; } = new();
            public HashSet<string> IgnoredMembers { get; set; } = new();
        }

        public PolicyEngine(Compilation compilation, MethodDeclarationSyntax? policyMethod, IReadOnlyList<INamedTypeSymbol> availableMappers)
        {
            _compilation = compilation;
            _policyMethod = policyMethod;
            _availableMappers = availableMappers;
            _analysisHelper = new AnalysisHelper(compilation, availableMappers);

            if (_policyMethod != null)
            {
                _semanticModel = compilation.GetSemanticModel(_policyMethod.SyntaxTree);
            }
        }

        public MappingPlan CreateBuildPlan(ITypeSymbol sourceType, ITypeSymbol destinationType)
        {
            var policy = _policyMethod != null ? ParsePolicy("ForCreation") : CreateDefaultBuildPolicy();
            return BuildPlanFromPolicy(sourceType, destinationType, policy);
        }

        public MappingPlan CreateApplyToPlan(ITypeSymbol sourceType, ITypeSymbol destinationType)
        {
            var policy = _policyMethod != null ? ParsePolicy("ForModification") : CreateDefaultApplyToPolicy();
            return BuildPlanFromPolicy(sourceType, destinationType, policy, isApplyTo: true);
        }

        public MappingPlan CreateProjectAsPlan(ITypeSymbol sourceType, ITypeSymbol destinationType)
        {
            var policy = _policyMethod != null ? ParsePolicy("ForProjection") : CreateDefaultProjectAsPolicy();
            return BuildPlanFromPolicy(sourceType, destinationType, policy, isProjectAs: true);
        }

        private MappingPlan BuildPlanFromPolicy(ITypeSymbol sourceType, ITypeSymbol destinationType, ParsedPolicy policy, bool isApplyTo = false, bool isProjectAs = false)
        {
            var plan = new MappingPlan(sourceType, destinationType);
            var destinationProperties = destinationType.GetMembers().OfType<IPropertySymbol>().ToDictionary(p => p.Name);

            plan.Creation.CustomParameterExpressions = policy.ParameterMappings;

            // Apply custom member mappings from policy.MemberMappings
            foreach (var memberConfig in policy.MemberMappings)
            {
                if (destinationProperties.TryGetValue(memberConfig.Key, out var destProp))
                {
                    plan.Instructions.Add(new CustomExpressionMapping(memberConfig.Value, destProp));
                }
            }

            var strategies = policy.Strategies.Any() ? policy.Strategies : CreateDefaultBuildPolicy().Strategies;

            foreach (var strategy in strategies)
            {
                if (plan.Creation.Method != CreationMethod.ParameterlessConstructor && plan.Creation.Method != CreationMethod.None &&
                    (strategy == MappingStrategy.UsePublicConstructors || strategy == MappingStrategy.UseStaticFactories))
                {
                    continue;
                }

                switch (strategy)
                {
                    case MappingStrategy.UsePublicConstructors when !isApplyTo:
                        _analysisHelper.FindBestConstructor(sourceType, destinationType, plan);
                        break;
                    case MappingStrategy.UseStaticFactories when !isApplyTo:
                        _analysisHelper.FindBestFactory(sourceType, destinationType, plan);
                        break;
                    case MappingStrategy.UsePublicSetters:
                        var ignored = isApplyTo ? policy.IgnoredMembers.Concat(new[] { "Id" }) : policy.IgnoredMembers;
                        _analysisHelper.MapRemainingProperties(sourceType, destinationType, plan, ignored);
                        break;
                }
            }

            return plan;
        }

        #region Default Policy Factories
        private ParsedPolicy CreateDefaultBuildPolicy() => new() { Strategies = new List<MappingStrategy> { MappingStrategy.UsePublicConstructors, MappingStrategy.UseStaticFactories, MappingStrategy.UsePublicSetters } };
        private ParsedPolicy CreateDefaultApplyToPolicy() => new() { Strategies = new List<MappingStrategy> { MappingStrategy.UsePublicSetters }, IgnoredMembers = new HashSet<string> { "Id" } };
        private ParsedPolicy CreateDefaultProjectAsPolicy() => new() { Strategies = new List<MappingStrategy> { MappingStrategy.UsePublicConstructors, MappingStrategy.UsePublicSetters } };
        #endregion

        #region Policy Parsing Logic
        private ParsedPolicy ParsePolicy(string initialMethodName)
        {
            var policy = new ParsedPolicy();
            if (_policyMethod?.Body == null || _semanticModel == null) return policy;

            var invocations = _policyMethod.Body.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (var invocation in invocations)
            {
                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) continue;
                var symbol = _semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                if (symbol == null || !IsPartOfCorrectPolicyChain(memberAccess.Expression, initialMethodName)) continue;

                switch (symbol.Name)
                {
                    case "Try":
                        if (invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression is ExpressionSyntax arg)
                        {
                            var constValue = _semanticModel.GetConstantValue(arg);
                            if (constValue.HasValue && constValue.Value is int strategyValue)
                                policy.Strategies.Add((MappingStrategy)strategyValue);
                        }
                        break;
                    case "Ignore":
                    case "IgnoreId":
                        if (invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression is LambdaExpressionSyntax lambda)
                            policy.IgnoredMembers.Add(GetMemberNameFromExpression(lambda));
                        else if (symbol.Name == "IgnoreId")
                            policy.IgnoredMembers.Add("Id");
                        break;
                    case "FromSource":
                        if (memberAccess.Expression is InvocationExpressionSyntax mapInvocation &&
                            mapInvocation.ArgumentList.Arguments.FirstOrDefault()?.Expression is LambdaExpressionSyntax destLambda &&
                            invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression is LambdaExpressionSyntax sourceLambda)
                        {
                            var mapSymbol = _semanticModel.GetSymbolInfo(mapInvocation).Symbol as IMethodSymbol;
                            var destMemberName = GetMemberNameFromExpression(destLambda);
                            if (mapSymbol != null && !string.IsNullOrEmpty(destMemberName))
                            {
                                if (mapSymbol.Name == "Map")
                                    policy.MemberMappings[destMemberName] = sourceLambda;
                                else if (mapSymbol.Name == "MapParameter")
                                    policy.ParameterMappings[destMemberName] = sourceLambda;
                            }
                        }
                        break;
                }
            }
            return policy;
        }

        private string GetMemberNameFromExpression(ExpressionSyntax expression)
        {
            if (expression is LambdaExpressionSyntax lambda)
            {
                if (lambda.Body is not ExpressionSyntax bodyExpression) return string.Empty;
                expression = bodyExpression;
            }
            if (expression is MemberAccessExpressionSyntax member) return member.Name.Identifier.Text;
            if (expression is CastExpressionSyntax cast && cast.Expression is MemberAccessExpressionSyntax castMember) return castMember.Name.Identifier.Text;
            if (expression is LiteralExpressionSyntax literal) return literal.Token.ValueText;
            return string.Empty;
        }

        private bool IsPartOfCorrectPolicyChain(ExpressionSyntax expression, string initialMethodName)
        {
            if (expression is InvocationExpressionSyntax invocation)
            {
                var symbol = _semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                if (symbol?.Name == initialMethodName) return true;
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                    return IsPartOfCorrectPolicyChain(memberAccess.Expression, initialMethodName);
            }
            else if (expression is IdentifierNameSyntax identifier)
            {
                return _semanticModel.GetSymbolInfo(identifier).Symbol is IParameterSymbol;
            }
            return false;
        }
        #endregion
    }
}

