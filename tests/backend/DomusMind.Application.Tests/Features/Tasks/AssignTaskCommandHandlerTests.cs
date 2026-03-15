using DomusMind.Application.Features.Tasks;
using DomusMind.Application.Features.Tasks.AssignTask;
using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Tasks;

public sealed class AssignTaskCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static AssignTaskCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubTasksAuthorizationService? auth = null)
        => new(db, new EventLogWriter(db), auth ?? new StubTasksAuthorizationService());

    private static async Task<(DomusMindDbContext Db, HouseholdTask Task)> BuildWithTaskAsync()
    {
        var db = CreateDb();
        var task = HouseholdTask.Create(
            TaskId.New(), FamilyId.New(),
            TaskTitle.Create("Fix the leak"), null, null, DateTime.UtcNow);
        db.Set<HouseholdTask>().Add(task);
        await db.SaveChangesAsync();
        task.ClearDomainEvents();
        return (db, task);
    }

    [Fact]
    public async Task Handle_WithValidInput_ReturnsResponse()
    {
        var (db, task) = await BuildWithTaskAsync();
        var memberId = Guid.NewGuid();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new AssignTaskCommand(task.Id.Value, memberId, Guid.NewGuid()),
            CancellationToken.None);

        result.TaskId.Should().Be(task.Id.Value);
        result.AssigneeId.Should().Be(memberId);
    }

    [Fact]
    public async Task Handle_TaskNotFound_ThrowsTasksException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new AssignTaskCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
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
            new AssignTaskCommand(task.Id.Value, Guid.NewGuid(), Guid.NewGuid()),
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
            new AssignTaskCommand(task.Id.Value, Guid.NewGuid(), Guid.NewGuid()),
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
            new AssignTaskCommand(task.Id.Value, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.TaskAlreadyCancelled);
    }
}
