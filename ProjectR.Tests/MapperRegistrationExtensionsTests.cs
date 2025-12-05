using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ProjectR.DI;
using ProjectR.Services;
using System.Reflection;
using Xunit;

namespace ProjectR.Tests.DI;

public class MapperRegistrationExtensionsTests
{
    private class TestEntity { }
    private class TestDto { }
    private class TestEntity2 { }
    private class TestDto2 { }

    private class TestMapper : Mapper<TestEntity, TestDto>
    {
        public override TestDto ProjectGenerated(TestEntity source) => new TestDto();
        public override TestEntity BuildGenerated(TestDto dto) => new TestEntity();
    }

    private class TestMapper2 : Mapper<TestEntity2, TestDto2>
    {
        public override TestDto2 ProjectGenerated(TestEntity2 source) => new TestDto2();
        public override TestEntity2 BuildGenerated(TestDto2 dto) => new TestEntity2();
    }

    private abstract class AbstractMapper : Mapper<TestEntity, TestDto>
    {
        // Abstract class should be ignored during scanning
    }

    private class NonMapperClass { }

    public class MapperRegistryTests
    {
        [Fact]
        public void Constructor_WithAssemblyContainingMappers_FindsMappers()
        {
            // Arrange
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            var registry = new MapperRegistrationExtensions.MapperRegistry(new[] { assembly });

            // Assert
            registry.FoundMappers.Should().NotBeEmpty();
            registry.FoundMappers.Should().Contain(typeof(TestMapper));
            registry.FoundMappers.Should().Contain(typeof(TestMapper2));
        }

        [Fact]
        public void Constructor_WithAssemblyContainingMappers_IgnoresAbstractMappers()
        {
            // Arrange
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            var registry = new MapperRegistrationExtensions.MapperRegistry(new[] { assembly });

            // Assert
            registry.FoundMappers.Should().NotContain(typeof(AbstractMapper));
        }

        [Fact]
        public void Constructor_WithAssemblyContainingMappers_IgnoresNonMappers()
        {
            // Arrange
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            var registry = new MapperRegistrationExtensions.MapperRegistry(new[] { assembly });

            // Assert
            registry.FoundMappers.Should().NotContain(typeof(NonMapperClass));
            registry.FoundMappers.Should().NotContain(typeof(string));
        }

        [Fact]
        public void Constructor_WithEmptyAssemblyList_ReturnsEmptyCollection()
        {
            // Arrange
            var assemblies = Array.Empty<Assembly>();

            // Act
            var registry = new MapperRegistrationExtensions.MapperRegistry(assemblies);

            // Assert
            registry.FoundMappers.Should().BeEmpty();
        }

        [Fact]
        public void FoundMappers_IsReadOnlyCollection()
        {
            // Arrange
            var assembly = Assembly.GetExecutingAssembly();
            var registry = new MapperRegistrationExtensions.MapperRegistry(new[] { assembly });

            // Act & Assert
            registry.FoundMappers.Should().BeAssignableTo<IReadOnlyCollection<Type>>();
        }
    }

    public class AddMappersExtensionTests
    {
        [Fact]
        public void AddMappers_WithValidAssembly_RegistersAllServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            services.AddMappers(assembly);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            // Check that MapperTypeCache is registered
            var cache = serviceProvider.GetService<MapperTypeCache>();
            cache.Should().NotBeNull();

            // Check that IMapperResolver is registered
            var resolver = serviceProvider.GetService<IMapperResolver>();
            resolver.Should().NotBeNull();
            resolver.Should().BeOfType<MapperResolver>();

            // Check that individual mappers are registered
            var mapper1 = serviceProvider.GetService<TestMapper>();
            mapper1.Should().NotBeNull();

            var mapper2 = serviceProvider.GetService<TestMapper2>();
            mapper2.Should().NotBeNull();
        }

        [Fact]
        public void AddMappers_WithNullAssemblies_UsesCallingAssembly()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddMappers(null!);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var resolver = serviceProvider.GetService<IMapperResolver>();
            resolver.Should().NotBeNull();
        }

        [Fact]
        public void AddMappers_WithEmptyAssemblies_UsesCallingAssembly()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddMappers();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var resolver = serviceProvider.GetService<IMapperResolver>();
            resolver.Should().NotBeNull();
        }

        [Fact]
        public void AddMappers_ReturnsServiceCollection_ForChaining()
        {
            // Arrange
            var services = new ServiceCollection();
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            var result = services.AddMappers(assembly);

            // Assert
            result.Should().BeSameAs(services);
        }

        [Fact]
        public void AddMappers_RegistersMapperTypeCacheAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            services.AddMappers(assembly);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var cache1 = serviceProvider.GetService<MapperTypeCache>();
            var cache2 = serviceProvider.GetService<MapperTypeCache>();
            cache1.Should().BeSameAs(cache2);
        }

        [Fact]
        public void AddMappers_RegistersIndividualMappersAsSingletons()
        {
            // Arrange
            var services = new ServiceCollection();
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            services.AddMappers(assembly);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var mapper1a = serviceProvider.GetService<TestMapper>();
            var mapper1b = serviceProvider.GetService<TestMapper>();
            mapper1a.Should().BeSameAs(mapper1b);
        }

        [Fact]
        public void AddMappers_RegistersResolverAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            services.AddMappers(assembly);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var resolver1 = serviceProvider.GetService<IMapperResolver>();
            var resolver2 = serviceProvider.GetService<IMapperResolver>();
            resolver1.Should().BeSameAs(resolver2);
        }
    }
}