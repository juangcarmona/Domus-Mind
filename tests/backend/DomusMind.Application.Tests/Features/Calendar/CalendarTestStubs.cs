using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Domain.Abstractions;

namespace DomusMind.Application.Tests.Features.Calendar;

internal sealed class StubCalendarAuthorizationService : IFamilyAuthorizationService
{
    public bool CanAccess { get; set; } = true;

    public Task<bool> CanAccessFamilyAsync(Guid userId, Guid familyId, CancellationToken cancellationToken = default)
        => Task.FromResult(CanAccess);
}

internal sealed class StubCalendarEventLogWriter : IEventLogWriter
{
    public List<IDomainEvent> WrittenEvents { get; } = [];

    public Task WriteAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        WrittenEvents.AddRange(domainEvents);
        return Task.CompletedTask;
    }
}
