using DomusMind.Application.Features.Tasks;
using DomusMind.Application.Features.Tasks.CancelTask;
using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.Enums;
using DomusMind.Domain.Tasks.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Tasks;

public sealed class CancelTaskCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static CancelTaskCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubTasksAuthorizationService? auth = null)
        => new(db, new EventLogWriter(db), auth ?? new StubTasksAuthorizationService());

    private static async Task<(DomusMindDbContext Db, HouseholdTask Task)> BuildWithTaskAsync()
    {
        var db = CreateDb();
        var task = HouseholdTask.Create(
            TaskId.New(), FamilyId.New(),
            TaskTitle.Create("Order supplies"), null, null, DateTime.UtcNow);
        db.Set<HouseholdTask>().Add(task);
        await db.SaveChangesAsync();
        task.ClearDomainEvents();
        return (db, task);
    }

    [Fact]
    public async Task Handle_WithValidInput_ReturnsCancelledStatus()
    {
        var (db, task) = await BuildWithTaskAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new CancelTaskCommand(task.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.TaskId.Should().Be(task.Id.Value);
        result.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task Handle_PersistsCancelledStatus()
    {
        var (db, task) = await BuildWithTaskAsync();
        var handler = BuildHandler(db);

        await handler.Handle(
            new CancelTaskCommand(task.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<HouseholdTask>()
            .SingleOrDefaultAsync(t => t.Id == task.Id);
        saved!.Status.Should().Be(HouseholdTaskStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_TaskNotFound_ThrowsTasksException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new CancelTaskCommand(Guid.NewGuid(), Guid.NewGuid()),
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
            new CancelTaskCommand(task.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_AlreadyCancelled_ThrowsTasksException()
    {
        var (db, task) = await BuildWithTaskAsync();
        task.Cancel();
        await db.SaveChangesAsync();
        task.ClearDomainEvents();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new CancelTaskCommand(task.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.TaskAlreadyCancelled);
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
            new CancelTaskCommand(task.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.TaskAlreadyCompleted);
    }
}
