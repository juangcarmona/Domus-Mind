using System.Text.Json;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Domain.Abstractions;
using DomusMind.Infrastructure.Persistence;

namespace DomusMind.Infrastructure.Events;

public sealed class EventLogWriter : IEventLogWriter
{
    private readonly DomusMindDbContext _dbContext;

    public EventLogWriter(DomusMindDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task WriteAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        if (domainEvents.Count == 0)
        {
            return;
        }

        var entries = domainEvents.Select(ToEntry).ToList();

        _dbContext.EventLog.AddRange(entries);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static EventLogEntry ToEntry(IDomainEvent domainEvent)
    {
        return new EventLogEntry
        {
            EventId = Guid.NewGuid(),
            EventType = domainEvent.GetType().Name,
            AggregateType = "unknown",
            AggregateId = "unknown",
            Module = InferModule(domainEvent.GetType().Namespace),
            OccurredAtUtc = domainEvent.OccurredAtUtc,
            Version = 1,
            PayloadJson = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
            CorrelationId = null,
            CausationId = null
        };
    }

    private static string InferModule(string? eventNamespace)
    {
        if (string.IsNullOrWhiteSpace(eventNamespace))
        {
            return "unknown";
        }

        if (eventNamespace.Contains(".Family.", StringComparison.Ordinal))
        {
            return "Family";
        }

        if (eventNamespace.Contains(".Responsibilities.", StringComparison.Ordinal))
        {
            return "Responsibilities";
        }

        if (eventNamespace.Contains(".Calendar.", StringComparison.Ordinal))
        {
            return "Calendar";
        }

        if (eventNamespace.Contains(".Tasks.", StringComparison.Ordinal))
        {
            return "Tasks";
        }

        return "unknown";
    }
}
