using DomusMind.Application.Abstractions.System;
using DomusMind.Application.Features.Setup;
using DomusMind.Application.Features.Setup.GetSetupStatus;
using FluentAssertions;

namespace DomusMind.Application.Tests.Features.Setup;

public sealed class GetSetupStatusQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenNotInitialized_ReturnsFalse()
    {
        var state = new StubSystemInitializationState(initialized: false);
        var handler = new GetSetupStatusQueryHandler(state);

        var result = await handler.Handle(new GetSetupStatusQuery(), CancellationToken.None);

        result.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenInitialized_ReturnsTrue()
    {
        var state = new StubSystemInitializationState(initialized: true);
        var handler = new GetSetupStatusQueryHandler(state);

        var result = await handler.Handle(new GetSetupStatusQuery(), CancellationToken.None);

        result.IsInitialized.Should().BeTrue();
    }

    // ── Stubs ─────────────────────────────────────────────────────────────────

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
}
