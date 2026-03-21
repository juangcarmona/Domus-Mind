using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family;
using DomusMind.Domain.Shared;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.ValueObjects;

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

/// <summary>Helpers for building HouseholdTask domain objects in tests.</summary>
internal static class TaskTestHelpers
{
    public static HouseholdTask MakeTask(
        FamilyId familyId,
        string title,
        DateOnly? dueDate = null,
        TimeOnly? dueTime = null)
    {
        TaskSchedule schedule = (dueDate, dueTime) switch
        {
            (null, _) => TaskSchedule.NoSchedule(),
            (not null, null) => TaskSchedule.WithDueDate(dueDate.Value),
            (not null, not null) => TaskSchedule.WithDueDateTime(dueDate.Value, dueTime.Value),
        };
        return HouseholdTask.Create(
            TaskId.New(), familyId,
            TaskTitle.Create(title), null,
            schedule,
            HexColor.From("#3B82F6"),
            DateTime.UtcNow);
    }
}
