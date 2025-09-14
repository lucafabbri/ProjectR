using FluentAssertions;
using ProjectR.Services;
using Xunit;

namespace ProjectR.Tests.Services;

public class MapperTypeCacheTests
{
    private class TestEntity { }
    private class TestDto { }
    private class TestEntity2 { }
    private class TestDto2 { }

    private class TestMapper : Mapper<TestEntity, TestDto>
    {
        public override TestDto Project(TestEntity source) => new TestDto();
        public override TestEntity Build(TestDto dto) => new TestEntity();
        public override void Apply(TestDto dto, TestEntity entityToUpdate) { }
    }

    private class TestMapper2 : Mapper<TestEntity2, TestDto2>
    {
        public override TestDto2 Project(TestEntity2 source) => new TestDto2();
        public override TestEntity2 Build(TestDto2 dto) => new TestEntity2();
        public override void Apply(TestDto2 dto, TestEntity2 entityToUpdate) { }
    }

    private class NonMapperClass { }

    [Fact]
    public void Constructor_WithValidMappers_CreatesBidirectionalMappings()
    {
        // Arrange
        var mapperTypes = new[] { typeof(TestMapper), typeof(TestMapper2) };

        // Act
        var cache = new MapperTypeCache(mapperTypes);

        // Assert
        cache.MapperTypes.Should().HaveCount(4); // 2 mappers x 2 directions each
        
        // Forward mappings
        cache.MapperTypes.Should().ContainKey((typeof(TestEntity), typeof(TestDto)));
        cache.MapperTypes[(typeof(TestEntity), typeof(TestDto))].Should().Be(typeof(TestMapper));
        
        cache.MapperTypes.Should().ContainKey((typeof(TestEntity2), typeof(TestDto2)));
        cache.MapperTypes[(typeof(TestEntity2), typeof(TestDto2))].Should().Be(typeof(TestMapper2));
        
        // Reverse mappings
        cache.MapperTypes.Should().ContainKey((typeof(TestDto), typeof(TestEntity)));
        cache.MapperTypes[(typeof(TestDto), typeof(TestEntity))].Should().Be(typeof(TestMapper));
        
        cache.MapperTypes.Should().ContainKey((typeof(TestDto2), typeof(TestEntity2)));
        cache.MapperTypes[(typeof(TestDto2), typeof(TestEntity2))].Should().Be(typeof(TestMapper2));
    }

    [Fact]
    public void Constructor_WithEmptyList_CreatesEmptyCache()
    {
        // Arrange
        var mapperTypes = Array.Empty<Type>();

        // Act
        var cache = new MapperTypeCache(mapperTypes);

        // Assert
        cache.MapperTypes.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNonMapperTypes_IgnoresNonMappers()
    {
        // Arrange
        var mapperTypes = new[] { typeof(NonMapperClass), typeof(TestMapper) };

        // Act
        var cache = new MapperTypeCache(mapperTypes);

        // Assert
        cache.MapperTypes.Should().HaveCount(2); // Only TestMapper in both directions
        cache.MapperTypes.Should().ContainKey((typeof(TestEntity), typeof(TestDto)));
        cache.MapperTypes.Should().ContainKey((typeof(TestDto), typeof(TestEntity)));
    }

    [Fact]
    public void Constructor_WithMixedTypes_OnlyProcessesMappers()
    {
        // Arrange
        var mapperTypes = new[] 
        { 
            typeof(NonMapperClass), 
            typeof(TestMapper), 
            typeof(string), 
            typeof(TestMapper2) 
        };

        // Act
        var cache = new MapperTypeCache(mapperTypes);

        // Assert
        cache.MapperTypes.Should().HaveCount(4); // Only valid mappers in both directions
        var expectedKeys = new[]
        {
            (typeof(TestEntity), typeof(TestDto)),
            (typeof(TestDto), typeof(TestEntity)),
            (typeof(TestEntity2), typeof(TestDto2)),
            (typeof(TestDto2), typeof(TestEntity2))
        };
        
        foreach (var expectedKey in expectedKeys)
        {
            cache.MapperTypes.Should().ContainKey(expectedKey);
        }
    }

    [Fact]
    public void MapperTypes_IsReadOnlyDictionary()
    {
        // Arrange
        var mapperTypes = new[] { typeof(TestMapper) };
        var cache = new MapperTypeCache(mapperTypes);

        // Act & Assert
        cache.MapperTypes.Should().BeAssignableTo<IReadOnlyDictionary<(Type, Type), Type>>();
    }
}