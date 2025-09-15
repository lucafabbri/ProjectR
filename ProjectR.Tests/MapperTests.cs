using FluentAssertions;
using Xunit;

namespace ProjectR.Tests.Mapping;

public class MapperTests
{
    private class TestEntity 
    { 
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
    
    private class TestDto 
    { 
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    private class TestMapper : Mapper<TestEntity, TestDto>
    {
        public override TestDto Project(TestEntity source)
        {
            return new TestDto 
            { 
                Name = source.Name, 
                Value = source.Value 
            };
        }

        public override TestEntity Build(TestDto dto)
        {
            return new TestEntity 
            { 
                Name = dto.Name, 
                Value = dto.Value 
            };
        }

        public override void Apply(TestDto dto, TestEntity entityToUpdate)
        {
            entityToUpdate.Name = dto.Name;
            entityToUpdate.Value = dto.Value;
        }
    }

    private class TestMapperWithRefinement : Mapper<TestEntity, TestDto>
    {
        public override TestDto Project(TestEntity source)
        {
            return new TestDto 
            { 
                Name = source.Name, 
                Value = source.Value 
            };
        }

        public override TestEntity Build(TestDto dto)
        {
            return new TestEntity 
            { 
                Name = dto.Name, 
                Value = dto.Value 
            };
        }

        public override void Apply(TestDto dto, TestEntity entityToUpdate)
        {
            entityToUpdate.Name = dto.Name;
            entityToUpdate.Value = dto.Value;
        }

        public override TestEntity BuildRefiner(TestEntity entity, TestDto dto)
        {
            entity.Name = entity.Name.ToUpper();
            return entity;
        }

        public override TestDto ProjectAsRefiner(TestDto dto, TestEntity entity)
        {
            dto.Name = dto.Name.ToLower();
            return dto;
        }

        public override void ApplyToRefiner(TestEntity entity, TestDto dto)
        {
            entity.Name = entity.Name.Trim();
        }
    }

    [Fact]
    public void BuildFactoryFallback_ReturnsDefault()
    {
        // Arrange & Act
        var result = Mapper<TestEntity, TestDto>.BuildFactoryFallback(new TestDto());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void BuildRefiner_DefaultImplementation_ReturnsEntityUnchanged()
    {
        // Arrange
        var mapper = new TestMapper();
        var entity = new TestEntity { Name = "Test", Value = 42 };
        var dto = new TestDto { Name = "Different", Value = 100 };

        // Act
        var result = mapper.BuildRefiner(entity, dto);

        // Assert
        result.Should().BeSameAs(entity);
        result.Name.Should().Be("Test"); // Unchanged
        result.Value.Should().Be(42); // Unchanged
    }

    [Fact]
    public void ProjectAsRefiner_DefaultImplementation_ReturnsDtoUnchanged()
    {
        // Arrange
        var mapper = new TestMapper();
        var entity = new TestEntity { Name = "Test", Value = 42 };
        var dto = new TestDto { Name = "Different", Value = 100 };

        // Act
        var result = mapper.ProjectAsRefiner(dto, entity);

        // Assert
        result.Should().BeSameAs(dto);
        result.Name.Should().Be("Different"); // Unchanged
        result.Value.Should().Be(100); // Unchanged
    }

    [Fact]
    public void ApplyToRefiner_DefaultImplementation_DoesNothing()
    {
        // Arrange
        var mapper = new TestMapper();
        var entity = new TestEntity { Name = "Test", Value = 42 };
        var dto = new TestDto { Name = "Different", Value = 100 };

        // Act
        mapper.ApplyToRefiner(entity, dto);

        // Assert
        entity.Name.Should().Be("Test"); // Unchanged
        entity.Value.Should().Be(42); // Unchanged
    }

    [Fact]
    public void BuildRefiner_CustomImplementation_ModifiesEntity()
    {
        // Arrange
        var mapper = new TestMapperWithRefinement();
        var entity = new TestEntity { Name = "test", Value = 42 };
        var dto = new TestDto { Name = "Different", Value = 100 };

        // Act
        var result = mapper.BuildRefiner(entity, dto);

        // Assert
        result.Should().BeSameAs(entity);
        result.Name.Should().Be("TEST"); // Modified to uppercase
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ProjectAsRefiner_CustomImplementation_ModifiesDto()
    {
        // Arrange
        var mapper = new TestMapperWithRefinement();
        var entity = new TestEntity { Name = "Test", Value = 42 };
        var dto = new TestDto { Name = "DIFFERENT", Value = 100 };

        // Act
        var result = mapper.ProjectAsRefiner(dto, entity);

        // Assert
        result.Should().BeSameAs(dto);
        result.Name.Should().Be("different"); // Modified to lowercase
        result.Value.Should().Be(100);
    }

    [Fact]
    public void ApplyToRefiner_CustomImplementation_ModifiesEntity()
    {
        // Arrange
        var mapper = new TestMapperWithRefinement();
        var entity = new TestEntity { Name = "  test  ", Value = 42 };
        var dto = new TestDto { Name = "Different", Value = 100 };

        // Act
        mapper.ApplyToRefiner(entity, dto);

        // Assert
        entity.Name.Should().Be("test"); // Trimmed
        entity.Value.Should().Be(42);
    }
}

public class MapperExtensionsTests
{
    private class TestEntity 
    { 
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
    
    private class TestDto 
    { 
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    private class TestMapper : Mapper<TestEntity, TestDto>
    {
        public override TestDto Project(TestEntity source)
        {
            return new TestDto 
            { 
                Name = source.Name, 
                Value = source.Value 
            };
        }

        public override TestEntity Build(TestDto dto)
        {
            return new TestEntity 
            { 
                Name = dto.Name, 
                Value = dto.Value 
            };
        }

        public override void Apply(TestDto dto, TestEntity entityToUpdate)
        {
            entityToUpdate.Name = dto.Name;
            entityToUpdate.Value = dto.Value;
        }
    }

    [Fact]
    public void Apply_Extension_UpdatesEntityAndReturnsIt()
    {
        // Arrange
        var dto = new TestDto { Name = "Updated", Value = 100 };
        var entity = new TestEntity { Name = "Original", Value = 50 };

        // Act
        var result = dto.Apply<TestDto, TestEntity, TestMapper>(entity);

        // Assert
        result.Should().BeSameAs(entity);
        result.Name.Should().Be("Updated");
        result.Value.Should().Be(100);
    }

    [Fact]
    public void Project_Extension_CreatesAndReturnsDto()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test", Value = 42 };

        // Act
        var result = entity.Project<TestDto, TestEntity, TestMapper>();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<TestDto>();
        result.Name.Should().Be("Test");
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Build_Extension_CreatesAndReturnsEntity()
    {
        // Arrange
        var dto = new TestDto { Name = "Test", Value = 42 };

        // Act
        var result = dto.Build<TestDto, TestEntity, TestMapper>();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<TestEntity>();
        result.Name.Should().Be("Test");
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Apply_Extension_CreatesNewMapperInstance()
    {
        // Arrange
        var dto = new TestDto { Name = "Test", Value = 42 };
        var entity1 = new TestEntity { Name = "Original1", Value = 1 };
        var entity2 = new TestEntity { Name = "Original2", Value = 2 };

        // Act
        var result1 = dto.Apply<TestDto, TestEntity, TestMapper>(entity1);
        var result2 = dto.Apply<TestDto, TestEntity, TestMapper>(entity2);

        // Assert
        // Both operations should work independently
        result1.Name.Should().Be("Test");
        result1.Value.Should().Be(42);
        result2.Name.Should().Be("Test");
        result2.Value.Should().Be(42);
    }

    [Fact]
    public void Project_Extension_CreatesNewMapperInstance()
    {
        // Arrange
        var entity1 = new TestEntity { Name = "Test1", Value = 1 };
        var entity2 = new TestEntity { Name = "Test2", Value = 2 };

        // Act
        var result1 = entity1.Project<TestDto, TestEntity, TestMapper>();
        var result2 = entity2.Project<TestDto, TestEntity, TestMapper>();

        // Assert
        result1.Name.Should().Be("Test1");
        result1.Value.Should().Be(1);
        result2.Name.Should().Be("Test2");
        result2.Value.Should().Be(2);
    }

    [Fact]
    public void Build_Extension_CreatesNewMapperInstance()
    {
        // Arrange
        var dto1 = new TestDto { Name = "Test1", Value = 1 };
        var dto2 = new TestDto { Name = "Test2", Value = 2 };

        // Act
        var result1 = dto1.Build<TestDto, TestEntity, TestMapper>();
        var result2 = dto2.Build<TestDto, TestEntity, TestMapper>();

        // Assert
        result1.Name.Should().Be("Test1");
        result1.Value.Should().Be(1);
        result2.Name.Should().Be("Test2");
        result2.Value.Should().Be(2);
    }
}