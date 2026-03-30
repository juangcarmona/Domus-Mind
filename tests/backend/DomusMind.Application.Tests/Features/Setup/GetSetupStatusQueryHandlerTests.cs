using DomusMind.Application.Abstractions.Platform;
using DomusMind.Application.Abstractions.System;
using DomusMind.Application.Features.Setup;
using DomusMind.Application.Features.Setup.GetSetupStatus;
using FluentAssertions;

namespace DomusMind.Application.Tests.Features.Setup;

public sealed class GetSetupStatusQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenCloudHostedAndNotInitialized_ReturnsFalse()
    {
        var state = new StubSystemInitializationState(initialized: false);
        var handler = new GetSetupStatusQueryHandler(
            new StubDeploymentModeContext(DeploymentMode.CloudHosted), state);

        var result = await handler.Handle(new GetSetupStatusQuery(), CancellationToken.None);

        result.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenCloudHostedAndInitialized_ReturnsTrue()
    {
        var state = new StubSystemInitializationState(initialized: true);
        var handler = new GetSetupStatusQueryHandler(
            new StubDeploymentModeContext(DeploymentMode.CloudHosted), state);

        var result = await handler.Handle(new GetSetupStatusQuery(), CancellationToken.None);

        result.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenSingleInstance_AlwaysReturnsInitializedTrue()
    {
        var state = new StubSystemInitializationState(initialized: false);
        var handler = new GetSetupStatusQueryHandler(
            new StubDeploymentModeContext(DeploymentMode.SingleInstance), state);

        var result = await handler.Handle(new GetSetupStatusQuery(), CancellationToken.None);

        result.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenSingleInstance_DoesNotCheckInitializationState()
    {
        var state = new NeverCalledSystemInitializationState();
        var handler = new GetSetupStatusQueryHandler(
            new StubDeploymentModeContext(DeploymentMode.SingleInstance), state);

        var result = await handler.Handle(new GetSetupStatusQuery(), CancellationToken.None);

        result.IsInitialized.Should().BeTrue();
    }

    // ── Stubs ─────────────────────────────────────────────────────────────────

    private sealed class StubDeploymentModeContext : IDeploymentModeContext
    {
        public StubDeploymentModeContext(DeploymentMode mode) => Mode = mode;
        public DeploymentMode Mode { get; }
        public bool CanCreateHousehold => true;
        public bool InvitationsEnabled => false;
        public bool RequireInvitationForSignup => false;
        public bool EmailEnabled => false;
        public bool SupportsAdminTools => false;
        public int MaxHouseholdsPerDeployment => 0;
    }

    private sealed class StubSystemInitializationState : ISystemInitializationState
    {
        private bool _initialized;

        public StubSystemInitializationState(bool initialized) => _initialized = initialized;

        public Task<bool> IsInitializedAsync(CancellationToken ct) => Task.FromResult(_initialized);

        public Task MarkInitializedAsync(CancellationToken ct)
        {
            _initialized = true;
            return Task.CompletedTask;
        }
    }

    private sealed class NeverCalledSystemInitializationState : ISystemInitializationState
    {
        public Task<bool> IsInitializedAsync(CancellationToken ct)
            => throw new InvalidOperationException("IsInitializedAsync must not be called in SingleInstance mode.");

        public Task MarkInitializedAsync(CancellationToken ct)
            => throw new InvalidOperationException("MarkInitializedAsync must not be called in SingleInstance mode.");
    }
}
