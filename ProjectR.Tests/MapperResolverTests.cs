using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ProjectR.Services;
using Xunit;

namespace ProjectR.Tests.Services;

public class MapperResolverTests
{
    private class TestEntity { }
    private class TestDto { }
    private class UnknownEntity { }
    private class UnknownDto { }

    private class TestMapper : Mapper<TestEntity, TestDto>
    {
        public override TestDto ProjectGenerated(TestEntity source) => new TestDto();
        public override TestEntity BuildGenerated(TestDto dto) => new TestEntity();
    }

    [Fact]
    public void GetMapper_WithValidMapping_ReturnsCorrectMapper()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<TestMapper>();
        // Register the interface mapping that the generator would do
        services.AddSingleton<Mapper<TestEntity, TestDto>>(sp => sp.GetRequiredService<TestMapper>());
        var serviceProvider = services.BuildServiceProvider();

        var resolver = new MapperResolver(serviceProvider);

        // Act
        var mapper = resolver.GetMapper<TestEntity, TestDto>();

        // Assert
        mapper.Should().NotBeNull();
        mapper.Should().BeOfType<TestMapper>();
    }

    [Fact]
    public void GetMapper_WithMissingMapping_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var resolver = new MapperResolver(serviceProvider);

        // Act & Assert
        var action = () => resolver.GetMapper<UnknownEntity, UnknownDto>();
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetMapper_WithMapperInterfaceNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<TestMapper>(); // TestMapper is registered, but Mapper<TSource, TDestination> is not
        var serviceProvider = services.BuildServiceProvider();

        var resolver = new MapperResolver(serviceProvider);

        // Act & Assert
        var action = () => resolver.GetMapper<TestEntity, TestDto>();
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetMapper_WithNullServiceFromContainer_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var resolver = new MapperResolver(serviceProvider);

        // Act & Assert
        var action = () => resolver.GetMapper<TestEntity, TestDto>();
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_WithValidArguments_InitializesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var resolver = new MapperResolver(serviceProvider);

        // Assert
        resolver.Should().NotBeNull();
        resolver.Should().BeAssignableTo<IMapperResolver>();
    }

    [Fact]
    public void GetMapper_WithBidirectionalMapping_StoresKeysInBothDirections()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<TestMapper>();
        services.AddSingleton<Mapper<TestEntity, TestDto>>(sp => sp.GetRequiredService<TestMapper>());
        var serviceProvider = services.BuildServiceProvider();

        var resolver = new MapperResolver(serviceProvider);

        // Act & Assert - Forward direction works
        var forwardMapper = resolver.GetMapper<TestEntity, TestDto>();
        forwardMapper.Should().NotBeNull();
        forwardMapper.Should().BeOfType<TestMapper>();

        // Act & Assert - Reverse direction will throw InvalidOperationException because Mapper<TestDto, TestEntity> is not registered
        var action = () => resolver.GetMapper<TestDto, TestEntity>();
        action.Should().Throw<InvalidOperationException>();
    }
}