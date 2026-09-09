using DomusMind.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace DomusMind.Infrastructure.Messaging;

public sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Dispatch<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<,>)
            .MakeGenericType(command.GetType(), typeof(TResponse));

        var handler = _serviceProvider.GetService(handlerType);
        if (handler is null)
        {
            throw new HandlerResolutionException(
                handlerType.FullName ?? handlerType.Name,
                $"No command handler registration found for {handlerType.Name}.");
        }

        var handleMethod = handlerType.GetMethod(
            nameof(ICommandHandler<ICommand<TResponse>, TResponse>.Handle))
            ?? throw new InvalidOperationException(
                $"Handle method not found on {handlerType.Name}.");

        var task = (Task<TResponse>?)handleMethod.Invoke(handler, [command, cancellationToken])
            ?? throw new InvalidOperationException(
                $"Handler {handlerType.Name} returned null.");

        return await task;
    }
}