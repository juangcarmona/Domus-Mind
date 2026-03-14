using DomusMind.Domain.Abstractions;

namespace DomusMind.Application.Abstractions.Messaging;

public interface IDomainEventDispatcher
{
    Task Dispatch(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default);
}
