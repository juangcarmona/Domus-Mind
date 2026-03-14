using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DomusMind.Infrastructure.Messaging;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Dispatch(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IDomainEventHandler<>)
                .MakeGenericType(domainEvent.GetType());

            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers.Where(h => h is not null))
            {
                await DispatchToHandler(handler!, domainEvent, cancellationToken);
            }
        }
    }

    private static Task DispatchToHandler(
        object handler,
        IDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        return ((dynamic)handler).Handle((dynamic)domainEvent, cancellationToken);
    }
}