using FluentAssertions;
using ProjectR.Policies;
using Xunit;

namespace ProjectR.Tests.Policies;

public class PolicyImplementationTests
{
    [Fact]
    public void CustomCreationPolicy_ImplementsICreationPolicy()
    {
        // Arrange
        var strategies = new List<MappingStrategy> { MappingStrategy.UsePublicConstructors };

        // Act
        var policy = new CustomCreationPolicy(strategies);

        // Assert
        policy.Should().BeAssignableTo<ICreationPolicy>();
    }

    [Fact]
    public void CustomCreationPolicy_Constructor_SetsStrategiesAsReadOnly()
    {
        // Arrange
        var strategies = new List<MappingStrategy> 
        { 
            MappingStrategy.UsePublicConstructors, 
            MappingStrategy.UseStaticFactories 
        };

        // Act
        var policy = new CustomCreationPolicy(strategies);

        // Assert
        policy.Strategies.Should().BeAssignableTo<IReadOnlyList<MappingStrategy>>();
        policy.Strategies.Should().Equal(MappingStrategy.UsePublicConstructors, MappingStrategy.UseStaticFactories);
        policy.Strategies.Should().HaveCount(2);
    }

    [Fact]
    public void CustomCreationPolicy_Constructor_WithEmptyList_CreatesEmptyReadOnlyList()
    {
        // Arrange
        var strategies = new List<MappingStrategy>();

        // Act
        var policy = new CustomCreationPolicy(strategies);

        // Assert
        policy.Strategies.Should().BeEmpty();
        policy.Strategies.Should().BeAssignableTo<IReadOnlyList<MappingStrategy>>();
    }

    [Fact]
    public void CustomCreationPolicy_StrategiesIsReadOnly_WrapAroundOriginalList()
    {
        // Arrange
        var strategies = new List<MappingStrategy> { MappingStrategy.UsePublicConstructors };
        var policy = new CustomCreationPolicy(strategies);

        // Act
        strategies.Add(MappingStrategy.UseStaticFactories);

        // Assert - The policy's strategies reflects changes to the original list (wrapped reference)
        policy.Strategies.Should().HaveCount(2);
        policy.Strategies.Should().Equal(MappingStrategy.UsePublicConstructors, MappingStrategy.UseStaticFactories);
    }

    [Fact]
    public void CustomModificationPolicy_ImplementsIModificationPolicy()
    {
        // Arrange
        var strategies = new List<MappingStrategy> { MappingStrategy.UsePublicSetters };

        // Act
        var policy = new CustomModificationPolicy(strategies);

        // Assert
        policy.Should().BeAssignableTo<IModificationPolicy>();
    }

    [Fact]
    public void CustomModificationPolicy_Constructor_SetsStrategiesAsReadOnly()
    {
        // Arrange
        var strategies = new List<MappingStrategy> 
        { 
            MappingStrategy.UsePublicSetters, 
            MappingStrategy.UseStaticFactories 
        };

        // Act
        var policy = new CustomModificationPolicy(strategies);

        // Assert
        policy.Strategies.Should().BeAssignableTo<IReadOnlyList<MappingStrategy>>();
        policy.Strategies.Should().Equal(MappingStrategy.UsePublicSetters, MappingStrategy.UseStaticFactories);
        policy.Strategies.Should().HaveCount(2);
    }

    [Fact]
    public void CustomModificationPolicy_Constructor_WithEmptyList_CreatesEmptyReadOnlyList()
    {
        // Arrange
        var strategies = new List<MappingStrategy>();

        // Act
        var policy = new CustomModificationPolicy(strategies);

        // Assert
        policy.Strategies.Should().BeEmpty();
        policy.Strategies.Should().BeAssignableTo<IReadOnlyList<MappingStrategy>>();
    }

    [Fact]
    public void CustomModificationPolicy_StrategiesIsReadOnly_WrapAroundOriginalList()
    {
        // Arrange
        var strategies = new List<MappingStrategy> { MappingStrategy.UsePublicSetters };
        var policy = new CustomModificationPolicy(strategies);

        // Act
        strategies.Add(MappingStrategy.UsePublicConstructors);

        // Assert - The policy's strategies reflects changes to the original list (wrapped reference)
        policy.Strategies.Should().HaveCount(2);
        policy.Strategies.Should().Equal(MappingStrategy.UsePublicSetters, MappingStrategy.UsePublicConstructors);
    }
}