using DomusMind.Application.Abstractions.Platform;
using DomusMind.Application.Features.Platform.GetCurrentDeploymentMode;
using FluentAssertions;

namespace DomusMind.Application.Tests.Features.Platform;

public sealed class GetCurrentDeploymentModeQueryHandlerTests
{
    private static GetCurrentDeploymentModeQueryHandler BuildHandler(
        IDeploymentModeContext? context = null,
        IHouseholdProvisioningPolicy? policy = null)
        => new(
            context ?? new StubDeploymentModeContext(DeploymentMode.SingleInstance),
            policy ?? new PermitAllPolicy());

    [Fact]
    public async Task Handle_ReturnsModeFromContext()
    {
        var handler = BuildHandler(
            context: new StubDeploymentModeContext(DeploymentMode.CloudHosted));

        var result = await handler.Handle(
            new GetCurrentDeploymentModeQuery(),
            CancellationToken.None);

        result.DeploymentMode.Should().Be("CloudHosted");
    }

    [Fact]
    public async Task Handle_WhenPolicyPermits_CanCreateHouseholdIsTrue()
    {
        var handler = BuildHandler(policy: new PermitAllPolicy());

        var result = await handler.Handle(
            new GetCurrentDeploymentModeQuery(),
            CancellationToken.None);

        result.CanCreateHousehold.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenPolicyDenies_CanCreateHouseholdIsFalse()
    {
        var handler = BuildHandler(policy: new DenyAllPolicy());

        var result = await handler.Handle(
            new GetCurrentDeploymentModeQuery(),
            CancellationToken.None);

        result.CanCreateHousehold.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ReflectsContextCapabilities()
    {
        var ctx = new StubDeploymentModeContext(
            DeploymentMode.SingleInstance,
            emailEnabled: true,
            supportsAdminTools: false,
            invitationsEnabled: false,
            requireInvitationForSignup: false);

        var handler = BuildHandler(context: ctx);
        var result = await handler.Handle(new GetCurrentDeploymentModeQuery(), CancellationToken.None);

        result.SupportsEmail.Should().BeTrue();
        result.SupportsAdminTools.Should().BeFalse();
        result.RequiresInvitation.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenRequireInvitationForSignup_RequiresInvitationIsTrue()
    {
        var ctx = new StubDeploymentModeContext(
            DeploymentMode.CloudHosted,
            invitationsEnabled: true,
            requireInvitationForSignup: true);

        var handler = BuildHandler(context: ctx);
        var result = await handler.Handle(new GetCurrentDeploymentModeQuery(), CancellationToken.None);

        result.RequiresInvitation.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_InvitationsEnabled_Without_RequireInvitationForSignup_RequiresInvitationIsFalse()
    {
        var ctx = new StubDeploymentModeContext(
            DeploymentMode.CloudHosted,
            invitationsEnabled: true,
            requireInvitationForSignup: false);

        var handler = BuildHandler(context: ctx);
        var result = await handler.Handle(new GetCurrentDeploymentModeQuery(), CancellationToken.None);

        result.RequiresInvitation.Should().BeFalse();
    }

    // ── Stubs ─────────────────────────────────────────────────────────────────

    private sealed class StubDeploymentModeContext : IDeploymentModeContext
    {
        public StubDeploymentModeContext(
            DeploymentMode mode,
            bool canCreateHousehold = true,
            bool invitationsEnabled = false,
            bool requireInvitationForSignup = false,
            bool emailEnabled = false,
            bool supportsAdminTools = false)
        {
            Mode = mode;
            CanCreateHousehold = canCreateHousehold;
            InvitationsEnabled = invitationsEnabled;
            RequireInvitationForSignup = requireInvitationForSignup;
            EmailEnabled = emailEnabled;
            SupportsAdminTools = supportsAdminTools;
        }

        public DeploymentMode Mode { get; }
        public bool CanCreateHousehold { get; }
        public bool InvitationsEnabled { get; }
        public bool RequireInvitationForSignup { get; }
        public bool EmailEnabled { get; }
        public bool SupportsAdminTools { get; }
        public int MaxHouseholdsPerDeployment => 0;
    }

    private sealed class PermitAllPolicy : IHouseholdProvisioningPolicy
    {
        public Task<ProvisioningPolicyResult> EvaluateAsync(CancellationToken ct)
            => Task.FromResult(ProvisioningPolicyResult.Permit());
    }

    private sealed class DenyAllPolicy : IHouseholdProvisioningPolicy
    {
        public Task<ProvisioningPolicyResult> EvaluateAsync(CancellationToken ct)
            => Task.FromResult(ProvisioningPolicyResult.DenySingleInstanceBound());
    }
}
