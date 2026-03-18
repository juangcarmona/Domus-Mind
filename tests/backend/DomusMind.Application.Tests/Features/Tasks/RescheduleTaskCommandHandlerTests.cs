using DomusMind.Application.Features.Tasks;
using DomusMind.Application.Features.Tasks.RescheduleTask;
using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Tasks;

public sealed class RescheduleTaskCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static RescheduleTaskCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubTasksAuthorizationService? auth = null)
        => new(db, new EventLogWriter(db), auth ?? new StubTasksAuthorizationService());

    private static async Task<(DomusMindDbContext Db, HouseholdTask Task)> BuildWithTaskAsync()
    {
        var db = CreateDb();
        var task = TaskTestHelpers.MakeTask(
            FamilyId.New(), "Schedule vet appointment",
            new DateOnly(2026, 4, 5));
        db.Set<HouseholdTask>().Add(task);
        await db.SaveChangesAsync();
        task.ClearDomainEvents();
        return (db, task);
    }

    [Fact]
    public async Task Handle_WithNewDueDate_ReturnsResponse()
    {
        var (db, task) = await BuildWithTaskAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new RescheduleTaskCommand(task.Id.Value, "2026-04-20", null, Guid.NewGuid()),
            CancellationToken.None);

        result.TaskId.Should().Be(task.Id.Value);
        result.DueDate.Should().Be("2026-04-20");
        result.DueTime.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithDueDateTime_ReturnsBothDateAndTime()
    {
        var (db, task) = await BuildWithTaskAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new RescheduleTaskCommand(task.Id.Value, "2026-04-20", "09:00", Guid.NewGuid()),
            CancellationToken.None);

        result.DueDate.Should().Be("2026-04-20");
        result.DueTime.Should().Be("09:00");
    }

    [Fact]
    public async Task Handle_RemoveDueDate_ClearsSchedule()
    {
        var (db, task) = await BuildWithTaskAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new RescheduleTaskCommand(task.Id.Value, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.DueDate.Should().BeNull();
        result.DueTime.Should().BeNull();
    }

    [Fact]
    public async Task Handle_PersistsNewDueDate()
    {
        var (db, task) = await BuildWithTaskAsync();
        var handler = BuildHandler(db);

        await handler.Handle(
            new RescheduleTaskCommand(task.Id.Value, "2026-05-01", null, Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<HouseholdTask>()
            .SingleOrDefaultAsync(t => t.Id == task.Id);
        saved!.Schedule.Date.Should().Be(new DateOnly(2026, 5, 1));
    }

    [Fact]
    public async Task Handle_TaskNotFound_ThrowsTasksException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new RescheduleTaskCommand(Guid.NewGuid(), "2026-04-20", null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.TaskNotFound);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsTasksException()
    {
        var (db, task) = await BuildWithTaskAsync();
        var auth = new StubTasksAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new RescheduleTaskCommand(task.Id.Value, "2026-04-20", null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_CompletedTask_ThrowsTasksException()
    {
        var (db, task) = await BuildWithTaskAsync();
        task.Complete();
        await db.SaveChangesAsync();
        task.ClearDomainEvents();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new RescheduleTaskCommand(task.Id.Value, "2026-04-20", null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.TaskAlreadyCompleted);
    }

    [Fact]
    public async Task Handle_CancelledTask_ThrowsTasksException()
    {
        var (db, task) = await BuildWithTaskAsync();
        task.Cancel();
        await db.SaveChangesAsync();
        task.ClearDomainEvents();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new RescheduleTaskCommand(task.Id.Value, "2026-04-20", null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.TaskAlreadyCancelled);
    }
}
