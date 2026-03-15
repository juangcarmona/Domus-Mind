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
        var task = HouseholdTask.Create(
            TaskId.New(), FamilyId.New(),
            TaskTitle.Create("Schedule vet appointment"),
            null, DateTime.UtcNow.AddDays(2), DateTime.UtcNow);
        db.Set<HouseholdTask>().Add(task);
        await db.SaveChangesAsync();
        task.ClearDomainEvents();
        return (db, task);
    }

    [Fact]
    public async Task Handle_WithNewDueDate_ReturnsResponse()
    {
        var (db, task) = await BuildWithTaskAsync();
        var newDue = DateTime.UtcNow.AddDays(7);
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new RescheduleTaskCommand(task.Id.Value, newDue, Guid.NewGuid()),
            CancellationToken.None);

        result.TaskId.Should().Be(task.Id.Value);
        result.NewDueDate.Should().Be(newDue);
    }

    [Fact]
    public async Task Handle_PersistsNewDueDate()
    {
        var (db, task) = await BuildWithTaskAsync();
        var newDue = DateTime.UtcNow.AddDays(14);
        var handler = BuildHandler(db);

        await handler.Handle(
            new RescheduleTaskCommand(task.Id.Value, newDue, Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<HouseholdTask>()
            .SingleOrDefaultAsync(t => t.Id == task.Id);
        saved!.DueDate.Should().Be(newDue);
    }

    [Fact]
    public async Task Handle_TaskNotFound_ThrowsTasksException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new RescheduleTaskCommand(Guid.NewGuid(), DateTime.UtcNow.AddDays(3), Guid.NewGuid()),
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
            new RescheduleTaskCommand(task.Id.Value, DateTime.UtcNow.AddDays(5), Guid.NewGuid()),
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
            new RescheduleTaskCommand(task.Id.Value, DateTime.UtcNow.AddDays(5), Guid.NewGuid()),
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
            new RescheduleTaskCommand(task.Id.Value, DateTime.UtcNow.AddDays(5), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.TaskAlreadyCancelled);
    }
}
