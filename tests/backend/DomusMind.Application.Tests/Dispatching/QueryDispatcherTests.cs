using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace DomusMind.Application.Tests.Dispatching;

public class QueryDispatcherTests
{
    [Fact]
    public async Task Dispatch_Should_Invoke_Handler()
    {
        var services = new ServiceCollection();

        services.AddScoped<IQueryHandler<TestQuery, int>, TestHandler>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        var provider = services.BuildServiceProvider();

        var dispatcher = provider.GetRequiredService<IQueryDispatcher>();

        var result = await dispatcher.Dispatch(new TestQuery());

        Assert.Equal(7, result);
    }

    private sealed class TestQuery : IQuery<int>;

    private sealed class TestHandler : IQueryHandler<TestQuery, int>
    {
        public Task<int> Handle(TestQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(7);
        }
    }
}
