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
        public override TestDto Project(TestEntity source) => new TestDto();
        public override TestEntity Build(TestDto dto) => new TestEntity();
        public override void Apply(TestDto dto, TestEntity entityToUpdate) { }
    }

    [Fact]
    public void GetMapper_WithValidMapping_ReturnsCorrectMapper()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<TestMapper>();
        var serviceProvider = services.BuildServiceProvider();

        var mapperTypeCache = new MapperTypeCache(new[] { typeof(TestMapper) });
        var resolver = new MapperResolver(serviceProvider, mapperTypeCache);

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

        var mapperTypeCache = new MapperTypeCache(Array.Empty<Type>());
        var resolver = new MapperResolver(serviceProvider, mapperTypeCache);

        // Act & Assert
        var action = () => resolver.GetMapper<UnknownEntity, UnknownDto>();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("No mapper registered for source 'UnknownEntity' and destination 'UnknownDto'.");
    }

    [Fact]
    public void GetMapper_WithServiceNotRegistered_ReturnsNullWithoutError()
    {
        // Arrange
        var services = new ServiceCollection();
        // Not registering TestMapper in DI container
        var serviceProvider = services.BuildServiceProvider();

        var mapperTypeCache = new MapperTypeCache(new[] { typeof(TestMapper) });
        var resolver = new MapperResolver(serviceProvider, mapperTypeCache);

        // Act
        var result = resolver.GetMapper<TestEntity, TestDto>();

        // Assert - GetService returns null, and casting null to reference type is valid
        result.Should().BeNull();
    }

    [Fact]
    public void GetMapper_WithNullServiceFromContainer_ReturnsNull()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(TestMapper))).Returns(null);

        var mapperTypeCache = new MapperTypeCache(new[] { typeof(TestMapper) });
        var resolver = new MapperResolver(mockServiceProvider.Object, mapperTypeCache);

        // Act
        var result = resolver.GetMapper<TestEntity, TestDto>();

        // Assert - Casting null to reference type is valid and returns null
        result.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithValidArguments_InitializesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var mapperTypeCache = new MapperTypeCache(Array.Empty<Type>());

        // Act
        var resolver = new MapperResolver(serviceProvider, mapperTypeCache);

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
        var serviceProvider = services.BuildServiceProvider();

        var mapperTypeCache = new MapperTypeCache(new[] { typeof(TestMapper) });
        var resolver = new MapperResolver(serviceProvider, mapperTypeCache);

        // Act & Assert - Forward direction works
        var forwardMapper = resolver.GetMapper<TestEntity, TestDto>();
        forwardMapper.Should().NotBeNull();
        forwardMapper.Should().BeOfType<TestMapper>();

        // Act & Assert - Reverse direction has the mapping key, but casting will fail
        // This tests that the cache contains the reverse mapping
        mapperTypeCache.MapperTypes.Should().ContainKey((typeof(TestDto), typeof(TestEntity)));
        
        // The actual reverse direction would fail due to type constraints, which is expected behavior
        var action = () => resolver.GetMapper<TestDto, TestEntity>();
        action.Should().Throw<InvalidCastException>();
    }
}