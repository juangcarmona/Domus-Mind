using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace DomusMind.Application.Tests.Dispatching;

public class CommandDispatcherTests
{
    [Fact]
    public async Task Dispatch_Should_Invoke_Handler()
    {
        var services = new ServiceCollection();

        services.AddScoped<ICommandHandler<TestCommand, int>, TestHandler>();
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();

        var provider = services.BuildServiceProvider();

        var dispatcher = provider.GetRequiredService<ICommandDispatcher>();

        var result = await dispatcher.Dispatch(new TestCommand());

        Assert.Equal(123, result);
    }

    private sealed class TestCommand : ICommand<int>;

    private sealed class TestHandler : ICommandHandler<TestCommand, int>
    {
        public Task<int> Handle(TestCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(123);
        }
    }
}
