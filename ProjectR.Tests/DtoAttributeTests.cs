using ProjectR.Attributes;
using FluentAssertions;
using Xunit;

namespace ProjectR.Tests.Attributes;

public class DtoAttributeTests
{
    private class TestEntity { }
    private class TestDto { }
    private class TestMapper { }

    [Fact]
    public void DtoAttribute_Constructor_SetsEntityTypeCorrectly()
    {
        // Arrange & Act
        var attribute = new DtoAttribute<TestEntity>();

        // Assert
        attribute.EntityType.Should().Be(typeof(TestEntity));
    }

    [Fact]
    public void DtoMapperAttribute_Constructor_SetsEntityTypeAndMapperTypeCorrectly()
    {
        // Arrange & Act
        var attribute = new DtoMapperAttribute<TestEntity, TestMapper>();

        // Assert
        attribute.EntityType.Should().Be(typeof(TestEntity));
        attribute.MapperType.Should().Be(typeof(TestMapper));
    }

    [Fact]
    public void DtoAttribute_HasCorrectAttributeUsage()
    {
        // Arrange & Act
        var attributeUsage = typeof(DtoAttribute<>).GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .First();

        // Assert
        attributeUsage.ValidOn.Should().Be(AttributeTargets.Class);
        attributeUsage.AllowMultiple.Should().BeTrue();
        attributeUsage.Inherited.Should().BeTrue();
    }

    [Fact]
    public void DtoMapperAttribute_HasCorrectAttributeUsage()
    {
        // Arrange & Act
        var attributeUsage = typeof(DtoMapperAttribute<,>).GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .First();

        // Assert
        attributeUsage.ValidOn.Should().Be(AttributeTargets.Class);
        attributeUsage.AllowMultiple.Should().BeTrue();
        attributeUsage.Inherited.Should().BeTrue();
    }

    [Fact]
    public void DtoMapperAttribute_InheritsFromDtoAttribute()
    {
        // Arrange & Act & Assert
        typeof(DtoMapperAttribute<TestEntity, TestMapper>).Should().BeDerivedFrom<DtoAttribute<TestEntity>>();
    }
}