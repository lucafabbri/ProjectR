using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using Xunit;

namespace ProjectR.Tests.Mapping;

public class MappingPlanTests
{
    [Fact]
    public void CreationMethod_Enum_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<CreationMethod>().Should().Contain(new[]
        {
            CreationMethod.None,
            CreationMethod.ParameterlessConstructor,
            CreationMethod.ConstructorWithParameters,
            CreationMethod.FactoryMethod
        });
    }

    [Fact]
    public void ParameterMappingInfo_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var mockSourceProperty = new Mock<IPropertySymbol>();
        var mockMapper = new Mock<INamedTypeSymbol>();

        // Act
        var parameterMappingInfo = new CreationInfo.ParameterMappingInfo(mockSourceProperty.Object, mockMapper.Object);

        // Assert
        parameterMappingInfo.SourceProperty.Should().Be(mockSourceProperty.Object);
        parameterMappingInfo.Mapper.Should().Be(mockMapper.Object);
    }

    [Fact]
    public void ParameterMappingInfo_Constructor_WithNullMapper_SetsPropertiesCorrectly()
    {
        // Arrange
        var mockSourceProperty = new Mock<IPropertySymbol>();

        // Act
        var parameterMappingInfo = new CreationInfo.ParameterMappingInfo(mockSourceProperty.Object, null);

        // Assert
        parameterMappingInfo.SourceProperty.Should().Be(mockSourceProperty.Object);
        parameterMappingInfo.Mapper.Should().BeNull();
    }

    [Fact]
    public void CreationInfo_Constructor_InitializesWithDefaults()
    {
        // Act
        var creationInfo = new CreationInfo();

        // Assert
        creationInfo.Method.Should().Be(CreationMethod.None);
        creationInfo.Constructor.Should().BeNull();
        creationInfo.FactoryMethod.Should().BeNull();
        creationInfo.ParametersMap.Should().NotBeNull().And.BeEmpty();
        creationInfo.CustomParameterExpressions.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void CreationInfo_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var creationInfo = new CreationInfo();
        var mockConstructor = new Mock<IMethodSymbol>();
        var mockFactoryMethod = new Mock<IMethodSymbol>();

        var lambdaExpression = SyntaxFactory.ParenthesizedLambdaExpression();

        // Act
        creationInfo.Method = CreationMethod.ConstructorWithParameters;
        creationInfo.Constructor = mockConstructor.Object;
        creationInfo.FactoryMethod = mockFactoryMethod.Object;
        creationInfo.CustomParameterExpressions["testParam"] = lambdaExpression;

        // Assert
        creationInfo.Method.Should().Be(CreationMethod.ConstructorWithParameters);
        creationInfo.Constructor.Should().Be(mockConstructor.Object);
        creationInfo.FactoryMethod.Should().Be(mockFactoryMethod.Object);
        creationInfo.ParametersMap.Should().NotBeNull();
        creationInfo.CustomParameterExpressions.Should().ContainKey("testParam").WhoseValue.Should().Be(lambdaExpression);
    }

    [Fact]
    public void MappingPlan_Constructor_SetsTypePropertiesCorrectly()
    {
        // Arrange
        var mockSourceType = new Mock<ITypeSymbol>();
        var mockDestinationType = new Mock<ITypeSymbol>();

        // Act
        var mappingPlan = new MappingPlan(mockSourceType.Object, mockDestinationType.Object);

        // Assert
        mappingPlan.SourceType.Should().Be(mockSourceType.Object);
        mappingPlan.DestinationType.Should().Be(mockDestinationType.Object);
    }

    [Fact]
    public void MappingPlan_Constructor_InitializesCollectionsAndDefaults()
    {
        // Arrange
        var mockSourceType = new Mock<ITypeSymbol>();
        var mockDestinationType = new Mock<ITypeSymbol>();

        // Act
        var mappingPlan = new MappingPlan(mockSourceType.Object, mockDestinationType.Object);

        // Assert
        mappingPlan.Creation.Should().NotBeNull();
        mappingPlan.Instructions.Should().NotBeNull().And.BeEmpty();
        mappingPlan.Diagnostics.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void MappingPlan_Collections_CanBeModified()
    {
        // Arrange
        var mockSourceType = new Mock<ITypeSymbol>();
        var mockDestinationType = new Mock<ITypeSymbol>();
        var mappingPlan = new MappingPlan(mockSourceType.Object, mockDestinationType.Object);

        var mockInstruction = new Mock<IMappingInstruction>();

        // Act
        mappingPlan.Instructions.Add(mockInstruction.Object);

        // Assert
        mappingPlan.Instructions.Should().HaveCount(1);
        mappingPlan.Instructions[0].Should().Be(mockInstruction.Object);
        mappingPlan.Diagnostics.Should().NotBeNull();
    }

    [Fact]
    public void MappingPlan_Creation_CanBeReplaced()
    {
        // Arrange
        var mockSourceType = new Mock<ITypeSymbol>();
        var mockDestinationType = new Mock<ITypeSymbol>();
        var mappingPlan = new MappingPlan(mockSourceType.Object, mockDestinationType.Object);
        var newCreationInfo = new CreationInfo { Method = CreationMethod.FactoryMethod };

        // Act
        mappingPlan.Creation = newCreationInfo;

        // Assert
        mappingPlan.Creation.Should().Be(newCreationInfo);
        mappingPlan.Creation.Method.Should().Be(CreationMethod.FactoryMethod);
    }
}