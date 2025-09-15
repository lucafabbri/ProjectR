using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace ProjectR.Tests.Diagnostic;

public class DiagnosticsTests
{
    [Fact]
    public void UnexpectedError_HasCorrectProperties()
    {
        // Arrange & Act
        var diagnostic = ProjectR.Diagnostics.UnexpectedError;

        // Assert
        diagnostic.Id.Should().Be("PR0001");
        diagnostic.Title.ToString().Should().Be("An unexpected error occurred");
        diagnostic.MessageFormat.ToString().Should().Be("An unexpected error occurred during mapper generation: '{0}' StackTrace: {1}");
        diagnostic.Category.Should().Be("ProjectR.Generator");
        diagnostic.DefaultSeverity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.IsEnabledByDefault.Should().BeTrue();
    }

    [Fact]
    public void MapperGenerationFailed_HasCorrectProperties()
    {
        // Arrange & Act
        var diagnostic = ProjectR.Diagnostics.MapperGenerationFailed;

        // Assert
        diagnostic.Id.Should().Be("PR0002");
        diagnostic.Title.ToString().Should().Be("Mapper generation failed");
        diagnostic.MessageFormat.ToString().Should().Be("Failed to generate an implementation for mapper '{0}'");
        diagnostic.Category.Should().Be("ProjectR.Generator");
        diagnostic.DefaultSeverity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.IsEnabledByDefault.Should().BeTrue();
    }

    [Fact]
    public void NoValidCreationMethod_HasCorrectProperties()
    {
        // Arrange & Act
        var diagnostic = ProjectR.Diagnostics.NoValidCreationMethod;

        // Assert
        diagnostic.Id.Should().Be("PR0003");
        diagnostic.Title.ToString().Should().Be("No valid creation method found");
        diagnostic.MessageFormat.ToString().Should().Be("ProjectR could not find a valid constructor or static factory method to create an instance of '{0}' based on the source type.");
        diagnostic.Category.Should().Be("ProjectR.Policies");
        diagnostic.DefaultSeverity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.IsEnabledByDefault.Should().BeTrue();
    }

    [Fact]
    public void UnmappableConstructorParameter_HasCorrectProperties()
    {
        // Arrange & Act
        var diagnostic = ProjectR.Diagnostics.UnmappableConstructorParameter;

        // Assert
        diagnostic.Id.Should().Be("PR0004");
        diagnostic.Title.ToString().Should().Be("Unmappable constructor parameter");
        diagnostic.MessageFormat.ToString().Should().Be("The parameter '{0}' of the constructor for '{1}' could not be mapped from any source property.");
        diagnostic.Category.Should().Be("ProjectR.Policies");
        diagnostic.DefaultSeverity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.IsEnabledByDefault.Should().BeTrue();
    }

    [Fact]
    public void UnmappedDestinationProperty_HasCorrectProperties()
    {
        // Arrange & Act
        var diagnostic = ProjectR.Diagnostics.UnmappedDestinationProperty;

        // Assert
        diagnostic.Id.Should().Be("PR0005");
        diagnostic.Title.ToString().Should().Be("Unmapped destination property");
        diagnostic.MessageFormat.ToString().Should().Be("The property '{0}' on destination type '{1}' was not mapped.");
        diagnostic.Category.Should().Be("ProjectR.Policies");
        diagnostic.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
        diagnostic.IsEnabledByDefault.Should().BeTrue();
    }
}