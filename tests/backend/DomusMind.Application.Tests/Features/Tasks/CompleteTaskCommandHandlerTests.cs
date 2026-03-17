using DomusMind.Application.Features.Tasks;
using DomusMind.Application.Features.Tasks.CompleteTask;
using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.Enums;
using DomusMind.Domain.Tasks.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Tasks;

public sealed class CompleteTaskCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static CompleteTaskCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubTasksAuthorizationService? auth = null)
        => new(db, new EventLogWriter(db), auth ?? new StubTasksAuthorizationService());

    private static async Task<(DomusMindDbContext Db, HouseholdTask Task)> BuildWithTaskAsync()
    {
        var db = CreateDb();
        var task = HouseholdTask.Create(
            TaskId.New(), FamilyId.New(),
            TaskTitle.Create("Water the plants"), null, null, DateTime.UtcNow);
        db.Set<HouseholdTask>().Add(task);
        await db.SaveChangesAsync();
        task.ClearDomainEvents();
        return (db, task);
    }

    [Fact]
    public async Task Handle_WithValidInput_ReturnsCompletedStatus()
    {
        var (db, task) = await BuildWithTaskAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new CompleteTaskCommand(task.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.TaskId.Should().Be(task.Id.Value);
        result.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task Handle_PersistsCompletedStatus()
    {
        var (db, task) = await BuildWithTaskAsync();
        var handler = BuildHandler(db);

        await handler.Handle(
            new CompleteTaskCommand(task.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<HouseholdTask>()
            .SingleOrDefaultAsync(t => t.Id == task.Id);
        saved!.Status.Should().Be(HouseholdTaskStatus.Completed);
    }

    [Fact]
    public async Task Handle_TaskNotFound_ThrowsTasksException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new CompleteTaskCommand(Guid.NewGuid(), Guid.NewGuid()),
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
            new CompleteTaskCommand(task.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_AlreadyCompleted_ThrowsTasksException()
    {
        var (db, task) = await BuildWithTaskAsync();
        task.Complete();
        await db.SaveChangesAsync();
        task.ClearDomainEvents();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new CompleteTaskCommand(task.Id.Value, Guid.NewGuid()),
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
            new CompleteTaskCommand(task.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.TaskAlreadyCancelled);
    }
}
