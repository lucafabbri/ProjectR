using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using Xunit;

namespace ProjectR.Tests.Mapping;

public class MappingInstructionTests
{
    [Fact]
    public void CustomExpressionMapping_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var sourceExpression = SyntaxFactory.ParenthesizedLambdaExpression();
        var mockDestination = new Mock<IPropertySymbol>();

        // Act
        var mapping = new CustomExpressionMapping(sourceExpression, mockDestination.Object);

        // Assert
        mapping.SourceExpression.Should().Be(sourceExpression);
        mapping.Destination.Should().Be(mockDestination.Object);
    }

    [Fact]
    public void CustomExpressionMapping_ImplementsIMappingInstruction()
    {
        // Arrange
        var sourceExpression = SyntaxFactory.ParenthesizedLambdaExpression();
        var mockDestination = new Mock<IPropertySymbol>();

        // Act
        var mapping = new CustomExpressionMapping(sourceExpression, mockDestination.Object);

        // Assert
        mapping.Should().BeAssignableTo<IMappingInstruction>();
    }

    [Fact]
    public void MethodCallMapping_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var mockSource1 = new Mock<IPropertySymbol>();
        var mockSource2 = new Mock<IPropertySymbol>();
        var sources = new[] { mockSource1.Object, mockSource2.Object };
        var mockUpdateMethod = new Mock<IMethodSymbol>();
        var mockDestination = new Mock<IPropertySymbol>();

        // Act
        var mapping = new MethodCallMapping(sources, mockUpdateMethod.Object, mockDestination.Object);

        // Assert
        mapping.Sources.Should().Equal(sources);
        mapping.UpdateMethod.Should().Be(mockUpdateMethod.Object);
        mapping.Destination.Should().Be(mockDestination.Object);
    }

    [Fact]
    public void MethodCallMapping_ImplementsIMappingInstruction()
    {
        // Arrange
        var sources = Array.Empty<IPropertySymbol>();
        var mockUpdateMethod = new Mock<IMethodSymbol>();
        var mockDestination = new Mock<IPropertySymbol>();

        // Act
        var mapping = new MethodCallMapping(sources, mockUpdateMethod.Object, mockDestination.Object);

        // Assert
        mapping.Should().BeAssignableTo<IMappingInstruction>();
    }

    [Fact]
    public void SimplePropertyMapping_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var mockSource = new Mock<IPropertySymbol>();
        var mockDestination = new Mock<IPropertySymbol>();

        // Act
        var mapping = new SimplePropertyMapping(mockSource.Object, mockDestination.Object);

        // Assert
        mapping.Source.Should().Be(mockSource.Object);
        mapping.Destination.Should().Be(mockDestination.Object);
    }

    [Fact]
    public void SimplePropertyMapping_ImplementsIMappingInstruction()
    {
        // Arrange
        var mockSource = new Mock<IPropertySymbol>();
        var mockDestination = new Mock<IPropertySymbol>();

        // Act
        var mapping = new SimplePropertyMapping(mockSource.Object, mockDestination.Object);

        // Assert
        mapping.Should().BeAssignableTo<IMappingInstruction>();
    }

    [Fact]
    public void CompositePropertyMapping_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var mockSource1 = new Mock<IPropertySymbol>();
        var mockSource2 = new Mock<IPropertySymbol>();
        var sources = new[] { mockSource1.Object, mockSource2.Object };
        var format = "{0} - {1}";
        var mockDestination = new Mock<IPropertySymbol>();

        // Act
        var mapping = new CompositePropertyMapping(sources, format, mockDestination.Object);

        // Assert
        mapping.Sources.Should().Equal(sources);
        mapping.Format.Should().Be(format);
        mapping.Destination.Should().Be(mockDestination.Object);
    }

    [Fact]
    public void CompositePropertyMapping_ImplementsIMappingInstruction()
    {
        // Arrange
        var sources = Array.Empty<IPropertySymbol>();
        var format = "test";
        var mockDestination = new Mock<IPropertySymbol>();

        // Act
        var mapping = new CompositePropertyMapping(sources, format, mockDestination.Object);

        // Assert
        mapping.Should().BeAssignableTo<IMappingInstruction>();
    }

    [Fact]
    public void NestedPropertyMapping_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var mockSource = new Mock<IPropertySymbol>();
        var mockMapper = new Mock<INamedTypeSymbol>();
        var mockDestination = new Mock<IPropertySymbol>();

        // Act
        var mapping = new NestedPropertyMapping(mockSource.Object, mockMapper.Object, mockDestination.Object);

        // Assert
        mapping.Source.Should().Be(mockSource.Object);
        mapping.Mapper.Should().Be(mockMapper.Object);
        mapping.Destination.Should().Be(mockDestination.Object);
    }

    [Fact]
    public void NestedPropertyMapping_ImplementsIMappingInstruction()
    {
        // Arrange
        var mockSource = new Mock<IPropertySymbol>();
        var mockMapper = new Mock<INamedTypeSymbol>();
        var mockDestination = new Mock<IPropertySymbol>();

        // Act
        var mapping = new NestedPropertyMapping(mockSource.Object, mockMapper.Object, mockDestination.Object);

        // Assert
        mapping.Should().BeAssignableTo<IMappingInstruction>();
    }

    [Fact]
    public void CollectionPropertyMapping_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var mockSource = new Mock<IPropertySymbol>();
        var mockElementMapper = new Mock<INamedTypeSymbol>();
        var mockDestination = new Mock<IPropertySymbol>();

        // Act
        var mapping = new CollectionPropertyMapping(mockSource.Object, mockElementMapper.Object, mockDestination.Object);

        // Assert
        mapping.Source.Should().Be(mockSource.Object);
        mapping.ElementMapper.Should().Be(mockElementMapper.Object);
        mapping.Destination.Should().Be(mockDestination.Object);
    }

    [Fact]
    public void CollectionPropertyMapping_ImplementsIMappingInstruction()
    {
        // Arrange
        var mockSource = new Mock<IPropertySymbol>();
        var mockElementMapper = new Mock<INamedTypeSymbol>();
        var mockDestination = new Mock<IPropertySymbol>();

        // Act
        var mapping = new CollectionPropertyMapping(mockSource.Object, mockElementMapper.Object, mockDestination.Object);

        // Assert
        mapping.Should().BeAssignableTo<IMappingInstruction>();
    }

    [Fact]
    public void MethodCallMapping_WithEmptySourcesList_WorksCorrectly()
    {
        // Arrange
        var sources = Array.Empty<IPropertySymbol>();
        var mockUpdateMethod = new Mock<IMethodSymbol>();
        var mockDestination = new Mock<IPropertySymbol>();

        // Act
        var mapping = new MethodCallMapping(sources, mockUpdateMethod.Object, mockDestination.Object);

        // Assert
        mapping.Sources.Should().BeEmpty();
        mapping.UpdateMethod.Should().Be(mockUpdateMethod.Object);
        mapping.Destination.Should().Be(mockDestination.Object);
    }

    [Fact]
    public void CompositePropertyMapping_WithEmptySourcesList_WorksCorrectly()
    {
        // Arrange
        var sources = Array.Empty<IPropertySymbol>();
        var format = "Empty format";
        var mockDestination = new Mock<IPropertySymbol>();

        // Act
        var mapping = new CompositePropertyMapping(sources, format, mockDestination.Object);

        // Assert
        mapping.Sources.Should().BeEmpty();
        mapping.Format.Should().Be(format);
        mapping.Destination.Should().Be(mockDestination.Object);
    }
}