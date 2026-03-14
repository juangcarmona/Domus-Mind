using DomusMind.Application.Abstractions.Messaging;

namespace DomusMind.Application.Tests.Messaging;

public class ICommandHandlerTests
{
    [Fact]
    public async Task Handler_Should_Return_Response()
    {
        var handler = new TestHandler();

        var result = await handler.Handle(new TestCommand(), CancellationToken.None);

        Assert.Equal(42, result);
    }

    private sealed class TestCommand : ICommand<int>;

    private sealed class TestHandler : ICommandHandler<TestCommand, int>
    {
        public Task<int> Handle(TestCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(42);
        }
    }
}
