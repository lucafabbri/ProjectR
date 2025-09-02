using Microsoft.CodeAnalysis;

namespace ProjectR
{
    internal static class Diagnostics
    {
        public static readonly DiagnosticDescriptor UnexpectedError = new(
            id: "PR0001",
            title: "An unexpected error occurred",
            messageFormat: "An unexpected error occurred during mapper generation: '{0}' StackTrace: {1}",
            category: "ProjectR.Generator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MapperGenerationFailed = new(
            id: "PR0002",
            title: "Mapper generation failed",
            messageFormat: "Failed to generate an implementation for mapper '{0}'",
            category: "ProjectR.Generator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor NoValidCreationMethod = new(
            id: "PR0003",
            title: "No valid creation method found",
            messageFormat: "ProjectR could not find a valid constructor or static factory method to create an instance of '{0}' based on the source type.",
            category: "ProjectR.Policies",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UnmappableConstructorParameter = new(
            id: "PR0004",
            title: "Unmappable constructor parameter",
            messageFormat: "The parameter '{0}' of the constructor for '{1}' could not be mapped from any source property.",
            category: "ProjectR.Policies",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UnmappedDestinationProperty = new(
            id: "PR0005",
            title: "Unmapped destination property",
            messageFormat: "The property '{0}' on destination type '{1}' was not mapped.",
            category: "ProjectR.Policies",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}

