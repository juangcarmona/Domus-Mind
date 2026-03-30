using DomusMind.Infrastructure.Platform;
using FluentAssertions;

namespace DomusMind.Application.Tests.Features.Platform;

public sealed class DeploymentSettingsValidationTests
{
    [Fact]
    public void Validate_WithValidSingleInstanceDefaults_DoesNotThrow()
    {
        var settings = new DeploymentSettings { Mode = "SingleInstance" };
        var act = () => settings.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithValidCloudHostedDefaults_DoesNotThrow()
    {
        var settings = new DeploymentSettings { Mode = "CloudHosted" };
        var act = () => settings.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithUnknownMode_ThrowsInvalidOperationException()
    {
        var settings = new DeploymentSettings { Mode = "Unknown" };
        var act = () => settings.Validate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Invalid Deployment:Mode value*");
    }

    [Fact]
    public void Validate_RequireInvitationWithInvitationsDisabled_ThrowsInvalidOperationException()
    {
        var settings = new DeploymentSettings
        {
            Mode = "CloudHosted",
            InvitationsEnabled = false,
            RequireInvitationForSignup = true
        };
        var act = () => settings.Validate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*RequireInvitationForSignup requires InvitationsEnabled*");
    }

    [Fact]
    public void ResolvedMode_WithSingleInstanceString_ReturnsSingleInstanceEnum()
    {
        var settings = new DeploymentSettings { Mode = "SingleInstance" };
        settings.ResolvedMode.Should().Be(DomusMind.Application.Abstractions.Platform.DeploymentMode.SingleInstance);
    }

    [Fact]
    public void Validate_SingleInstanceWithInvitationsEnabled_ThrowsInvalidOperationException()
    {
        var settings = new DeploymentSettings
        {
            Mode = "SingleInstance",
            InvitationsEnabled = true
        };
        var act = () => settings.Validate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*InvitationsEnabled is not supported in SingleInstance mode*");
    }

    [Fact]
    public void Validate_SingleInstanceWithRequireInvitationForSignup_ThrowsInvalidOperationException()
    {
        var settings = new DeploymentSettings
        {
            Mode = "SingleInstance",
            RequireInvitationForSignup = true
        };
        var act = () => settings.Validate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*RequireInvitationForSignup is not supported in SingleInstance mode*");
    }

    [Fact]
    public void Validate_ModeIsCaseInsensitive()
    {
        var settings = new DeploymentSettings { Mode = "singleinstance" };
        var act = () => settings.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void ResolvedMode_WithUnknownMode_DefaultsToSingleInstance()
    {
        var settings = new DeploymentSettings { Mode = "bogus" };
        settings.ResolvedMode.Should().Be(DomusMind.Application.Abstractions.Platform.DeploymentMode.SingleInstance);
    }

    [Fact]
    public void Validate_WithNegativeMaxHouseholds_ThrowsInvalidOperationException()
    {
        var settings = new DeploymentSettings { Mode = "CloudHosted", MaxHouseholdsPerDeployment = -1 };
        var act = () => settings.Validate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*MaxHouseholdsPerDeployment cannot be negative*");
    }

    [Fact]
    public void Validate_SingleInstanceWithMaxHouseholdsGreaterThanOne_ThrowsInvalidOperationException()
    {
        var settings = new DeploymentSettings { Mode = "SingleInstance", MaxHouseholdsPerDeployment = 2 };
        var act = () => settings.Validate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*MaxHouseholdsPerDeployment must be 0 or 1*");
    }

    [Fact]
    public void Validate_CloudHostedWithMaxHouseholdsEqualToOne_ThrowsInvalidOperationException()
    {
        var settings = new DeploymentSettings { Mode = "CloudHosted", MaxHouseholdsPerDeployment = 1 };
        var act = () => settings.Validate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*MaxHouseholdsPerDeployment = 1 is not valid for CloudHosted mode*");
    }

    [Fact]
    public void Validate_CloudHostedWithZeroMaxHouseholds_DoesNotThrow()
    {
        var settings = new DeploymentSettings { Mode = "CloudHosted", MaxHouseholdsPerDeployment = 0 };
        var act = () => settings.Validate();
        act.Should().NotThrow();
    }
}
