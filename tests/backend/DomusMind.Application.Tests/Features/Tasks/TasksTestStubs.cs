using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Domain.Abstractions;

namespace DomusMind.Application.Tests.Features.Tasks;

internal sealed class StubTasksAuthorizationService : IFamilyAuthorizationService
{
    public bool CanAccess { get; set; } = true;

    public Task<bool> CanAccessFamilyAsync(
        Guid userId, Guid familyId, CancellationToken cancellationToken = default)
        => Task.FromResult(CanAccess);
}

internal sealed class StubTasksEventLogWriter : IEventLogWriter
{
    public List<IDomainEvent> WrittenEvents { get; } = [];

    public Task WriteAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        WrittenEvents.AddRange(domainEvents);
        return Task.CompletedTask;
    }
}
