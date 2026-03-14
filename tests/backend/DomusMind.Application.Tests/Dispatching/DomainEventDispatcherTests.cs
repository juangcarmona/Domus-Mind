using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Domain.Abstractions;
using DomusMind.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace DomusMind.Application.Tests.Dispatching;

public class DomainEventDispatcherTests
{
    [Fact]
    public async Task Dispatch_Should_Invoke_All_Handlers()
    {
        var services = new ServiceCollection();

        services.AddScoped<IDomainEventHandler<TestEvent>, HandlerA>();
        services.AddScoped<IDomainEventHandler<TestEvent>, HandlerB>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        var provider = services.BuildServiceProvider();

        var dispatcher = provider.GetRequiredService<IDomainEventDispatcher>();

        HandlerA.Called = false;
        HandlerB.Called = false;

        await dispatcher.Dispatch(new[] { new TestEvent(DateTime.UtcNow) });

        Assert.True(HandlerA.Called);
        Assert.True(HandlerB.Called);
    }

    private sealed record TestEvent(DateTime OccurredAtUtc) : IDomainEvent;

    private sealed class HandlerA : IDomainEventHandler<TestEvent>
    {
        public static bool Called;

        public Task Handle(TestEvent domainEvent, CancellationToken cancellationToken)
        {
            Called = true;
            return Task.CompletedTask;
        }
    }

    private sealed class HandlerB : IDomainEventHandler<TestEvent>
    {
        public static bool Called;

        public Task Handle(TestEvent domainEvent, CancellationToken cancellationToken)
        {
            Called = true;
            return Task.CompletedTask;
        }
    }
}
