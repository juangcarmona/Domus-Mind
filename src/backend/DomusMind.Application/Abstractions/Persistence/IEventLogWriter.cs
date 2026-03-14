using DomusMind.Domain.Abstractions;

namespace DomusMind.Application.Abstractions.Persistence;

public interface IEventLogWriter
{
    Task WriteAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default);
}
