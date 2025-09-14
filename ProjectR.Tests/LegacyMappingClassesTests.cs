using FluentAssertions;
using Microsoft.CodeAnalysis;
using Moq;
using Xunit;

namespace ProjectR.Tests.Mapping;

public class LegacyMappingClassesTests
{
    [Fact]
    public void MappingInstruction_IsAbstractBaseClass()
    {
        // Assert
        typeof(MappingInstruction).Should().BeAbstract();
    }

    [Fact]
    public void NestedObjectMapperMapping_InheritsFromMappingInstruction()
    {
        // Assert
        typeof(NestedObjectMapperMapping).Should().BeDerivedFrom<MappingInstruction>();
    }

    [Fact]
    public void NestedObjectMapperMapping_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var mockSourceProperty = new Mock<IPropertySymbol>();
        var mockDestinationProperty = new Mock<IPropertySymbol>();
        var mockMapperType = new Mock<INamedTypeSymbol>();
        var mapping = new NestedObjectMapperMapping();

        // Act
        mapping.SourceProperty = mockSourceProperty.Object;
        mapping.DestinationProperty = mockDestinationProperty.Object;
        mapping.MapperType = mockMapperType.Object;

        // Assert
        mapping.SourceProperty.Should().Be(mockSourceProperty.Object);
        mapping.DestinationProperty.Should().Be(mockDestinationProperty.Object);
        mapping.MapperType.Should().Be(mockMapperType.Object);
    }

    [Fact]
    public void ParameterMapping_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var mapping = new ParameterMapping();
        var sourcePropertyName = "SourceProperty";
        var destinationParameterName = "destinationParam";

        // Act
        mapping.SourcePropertyName = sourcePropertyName;
        mapping.DestinationParameterName = destinationParameterName;

        // Assert
        mapping.SourcePropertyName.Should().Be(sourcePropertyName);
        mapping.DestinationParameterName.Should().Be(destinationParameterName);
    }

    [Fact]
    public void PropertyMapping_InheritsFromMappingInstruction()
    {
        // Assert
        typeof(PropertyMapping).Should().BeDerivedFrom<MappingInstruction>();
    }

    [Fact]
    public void PropertyMapping_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var mapping = new PropertyMapping();
        var sourcePropertyName = "SourceProperty";
        var destinationPropertyName = "DestinationProperty";

        // Act
        mapping.SourcePropertyName = sourcePropertyName;
        mapping.DestinationPropertyName = destinationPropertyName;

        // Assert
        mapping.SourcePropertyName.Should().Be(sourcePropertyName);
        mapping.DestinationPropertyName.Should().Be(destinationPropertyName);
    }

    [Fact]
    public void PropertyMappingInstruction_IsAbstractBaseClass()
    {
        // Assert
        typeof(PropertyMappingInstruction).Should().BeAbstract();
    }

    [Fact]
    public void PropertyMappingInstruction_InheritsFromMappingInstruction()
    {
        // Assert
        typeof(PropertyMappingInstruction).Should().BeDerivedFrom<MappingInstruction>();
    }

    [Fact]
    public void PropertyMappingInstruction_Constructor_SetsDestinationProperty()
    {
        // Arrange
        var mockDestinationProperty = new Mock<IPropertySymbol>();

        // Act
        var instruction = new TestPropertyMappingInstruction(mockDestinationProperty.Object);

        // Assert
        instruction.DestinationProperty.Should().Be(mockDestinationProperty.Object);
    }

    [Fact]
    public void PropertyMappingInstruction_DestinationProperty_IsReadOnly()
    {
        // Arrange
        var mockDestinationProperty = new Mock<IPropertySymbol>();
        var instruction = new TestPropertyMappingInstruction(mockDestinationProperty.Object);

        // Assert
        var property = typeof(PropertyMappingInstruction).GetProperty(nameof(PropertyMappingInstruction.DestinationProperty));
        property!.CanWrite.Should().BeFalse();
        property.CanRead.Should().BeTrue();
    }

    // Test implementation of abstract PropertyMappingInstruction
    private class TestPropertyMappingInstruction : PropertyMappingInstruction
    {
        public TestPropertyMappingInstruction(IPropertySymbol destinationProperty) : base(destinationProperty) { }
    }
}