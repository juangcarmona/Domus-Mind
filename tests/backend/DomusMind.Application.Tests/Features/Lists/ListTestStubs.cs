using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;

namespace DomusMind.Application.Tests.Features.Lists;

internal sealed class StubListAuthorizationService : IFamilyAuthorizationService
{
    public bool CanAccess { get; set; } = true;

    public Task<bool> CanAccessFamilyAsync(
        Guid userId, Guid familyId, CancellationToken cancellationToken = default)
        => Task.FromResult(CanAccess);
}

internal sealed class StubListEventLogWriter : IEventLogWriter
{
    public List<IDomainEvent> WrittenEvents { get; } = [];

    public Task WriteAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        WrittenEvents.AddRange(domainEvents);
        return Task.CompletedTask;
    }
}

internal static class ListTestHelpers
{
    public static SharedList MakeList(
        FamilyId familyId,
        string name = "Test List",
        string kind = "Shopping",
        DateTime? createdAtUtc = null)
        => SharedList.Create(
            ListId.New(),
            familyId,
            ListName.Create(name),
            ListKind.Create(kind),
            areaId: null,
            linkedEntityType: null,
            linkedEntityId: null,
            createdAtUtc: createdAtUtc ?? DateTime.UtcNow);
}
