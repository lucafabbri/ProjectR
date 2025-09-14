using FluentAssertions;
using Xunit;

namespace ProjectR.Tests.Policies;

public class PoliciesApiTests
{
    [Fact]
    public void MappingStrategy_Enum_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<MappingStrategy>().Should().Contain(new[]
        {
            MappingStrategy.UsePublicConstructors,
            MappingStrategy.UseStaticFactories,
            MappingStrategy.UsePublicSetters
        });
    }

    [Fact]
    public void MappingStrategy_HasCorrectValues()
    {
        // Assert
        ((int)MappingStrategy.UsePublicConstructors).Should().Be(1);
        ((int)MappingStrategy.UseStaticFactories).Should().Be(2);
        ((int)MappingStrategy.UsePublicSetters).Should().Be(3);
    }

    [Fact]
    public void IPolicyConfiguration_IsInterface()
    {
        // Assert
        typeof(IPolicyConfiguration).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IMemberConfiguration_IsInterface()
    {
        // Assert
        typeof(IMemberConfiguration<,>).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IPolicyBuilder_IsInterface()
    {
        // Assert
        typeof(IPolicyBuilder<,>).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IProjectAsPolicyBuilder_InheritsFromIPolicyBuilder()
    {
        // Assert
        typeof(IProjectAsPolicyBuilder<,>).Should().BeAssignableTo(typeof(IPolicyBuilder<,>));
    }

    [Fact]
    public void IBuildPolicyBuilder_InheritsFromIPolicyBuilder()
    {
        // Assert
        typeof(IBuildPolicyBuilder<,>).Should().BeAssignableTo(typeof(IPolicyBuilder<,>));
    }

    [Fact]
    public void IApplyToPolicyBuilder_InheritsFromIPolicyBuilder()
    {
        // Assert
        typeof(IApplyToPolicyBuilder<,>).Should().BeAssignableTo(typeof(IPolicyBuilder<,>));
    }

    [Fact]
    public void ICreationPolicy_IsInterface()
    {
        // Assert
        typeof(ICreationPolicy).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IModificationPolicy_IsInterface()
    {
        // Assert
        typeof(IModificationPolicy).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IMappingPolicy_IsInterface()
    {
        // Assert
        typeof(IMappingPolicy).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IMappingPolicy_IsMarkerInterface()
    {
        // Assert
        typeof(IMappingPolicy).GetMethods().Should().BeEmpty();
        typeof(IMappingPolicy).GetProperties().Should().BeEmpty();
    }
}