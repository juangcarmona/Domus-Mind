using DomusMind.Application.Features.Tasks;
using DomusMind.Application.Features.Tasks.ReassignTask;
using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.Events;
using DomusMind.Domain.Tasks.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Tasks;

public sealed class ReassignTaskCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ReassignTaskCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubTasksAuthorizationService? auth = null)
        => new(db, new EventLogWriter(db), auth ?? new StubTasksAuthorizationService());

    private static HouseholdTask BuildAssignedTask(FamilyId familyId, MemberId? assignee = null)
    {
        var task = HouseholdTask.Create(
            TaskId.New(), familyId,
            TaskTitle.Create("Fix Fence"), null,
            DateTime.UtcNow.AddDays(2), DateTime.UtcNow);
        if (assignee.HasValue)
            task.Assign(assignee.Value);
        task.ClearDomainEvents();
        return task;
    }

    [Fact]
    public async Task Handle_TaskNotFound_ThrowsTasksException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new ReassignTaskCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.TaskNotFound);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsTasksException()
    {
        var db = CreateDb();
        var auth = new StubTasksAuthorizationService { CanAccess = false };
        var familyId = FamilyId.New();
        var task = BuildAssignedTask(familyId);
        db.Set<HouseholdTask>().Add(task);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new ReassignTaskCommand(task.Id.Value, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_ValidReassign_ChangesAssigneeInDatabase()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var originalAssignee = MemberId.New();
        var newAssignee = MemberId.New();
        var task = BuildAssignedTask(familyId, originalAssignee);
        db.Set<HouseholdTask>().Add(task);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        await handler.Handle(
            new ReassignTaskCommand(task.Id.Value, newAssignee.Value, Guid.NewGuid()),
            CancellationToken.None);

        var updated = await db.Set<HouseholdTask>()
            .SingleAsync(t => t.Id == task.Id);
        updated.AssigneeId.Should().Be(newAssignee);
    }

    [Fact]
    public async Task Handle_ValidReassign_ReturnsResponseWithPreviousAndNewAssignee()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var originalAssignee = MemberId.New();
        var newAssignee = MemberId.New();
        var task = BuildAssignedTask(familyId, originalAssignee);
        db.Set<HouseholdTask>().Add(task);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new ReassignTaskCommand(task.Id.Value, newAssignee.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.PreviousAssigneeId.Should().Be(originalAssignee.Value);
        result.NewAssigneeId.Should().Be(newAssignee.Value);
    }

    [Fact]
    public async Task Handle_WritesTaskReassignedEventToLog()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var task = BuildAssignedTask(familyId, MemberId.New());
        db.Set<HouseholdTask>().Add(task);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        await handler.Handle(
            new ReassignTaskCommand(task.Id.Value, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        var logEntries = db.EventLog.ToList();
        logEntries.Should().Contain(e => e.EventType == nameof(TaskReassigned));
    }
}
