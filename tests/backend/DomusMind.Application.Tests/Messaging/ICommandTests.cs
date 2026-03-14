using DomusMind.Application.Abstractions.Messaging;

namespace DomusMind.Application.Tests.Messaging;

public class ICommandTests
{
    [Fact]
    public void Command_Should_Implement_ICommand_Interface()
    {
        var command = new TestCommand();

        Assert.IsAssignableFrom<ICommand<int>>(command);
    }

    private sealed class TestCommand : ICommand<int>;
}
