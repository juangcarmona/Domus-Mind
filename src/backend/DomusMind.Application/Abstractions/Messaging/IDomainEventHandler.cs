using DomusMind.Domain.Abstractions;

namespace DomusMind.Application.Abstractions.Messaging;

public interface IDomainEventHandler<TEvent>
    where TEvent : IDomainEvent
{
    Task Handle(
        TEvent domainEvent,
        CancellationToken cancellationToken);
}
