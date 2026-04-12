using DomusMind.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace DomusMind.Infrastructure.Messaging;

public sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Dispatch<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<,>)
            .MakeGenericType(query.GetType(), typeof(TResponse));

        var handler = _serviceProvider.GetService(handlerType);
        if (handler is null)
        {
            throw new HandlerResolutionException(
                handlerType.FullName ?? handlerType.Name,
                $"No query handler registration found for {handlerType.Name}.");
        }

        var handleMethod = handlerType.GetMethod(
            nameof(IQueryHandler<IQuery<TResponse>, TResponse>.Handle))
            ?? throw new InvalidOperationException(
                $"Handle method not found on {handlerType.Name}.");

        var task = (Task<TResponse>?)handleMethod.Invoke(handler, [query, cancellationToken])
            ?? throw new InvalidOperationException(
                $"Handler {handlerType.Name} returned null.");

        return await task;
    }
}