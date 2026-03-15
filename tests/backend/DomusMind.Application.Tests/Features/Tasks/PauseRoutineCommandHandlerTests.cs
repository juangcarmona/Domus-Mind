using DomusMind.Application.Features.Tasks;
using DomusMind.Application.Features.Tasks.PauseRoutine;
using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Tasks;

public sealed class PauseRoutineCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static PauseRoutineCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubTasksAuthorizationService? auth = null)
        => new(db, new EventLogWriter(db), auth ?? new StubTasksAuthorizationService());

    private static async Task<(DomusMindDbContext Db, Routine Routine)> BuildWithRoutineAsync()
    {
        var db = CreateDb();
        var routine = Routine.Create(
            RoutineId.New(), FamilyId.New(),
            RoutineName.Create("Grocery Shopping"), "Every Sunday at 11:00", DateTime.UtcNow);
        db.Set<Routine>().Add(routine);
        await db.SaveChangesAsync();
        routine.ClearDomainEvents();
        return (db, routine);
    }

    [Fact]
    public async Task Handle_WithActiveRoutine_ReturnsPausedStatus()
    {
        var (db, routine) = await BuildWithRoutineAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new PauseRoutineCommand(routine.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.RoutineId.Should().Be(routine.Id.Value);
        result.Status.Should().Be("Paused");
    }

    [Fact]
    public async Task Handle_PersistsPausedStatus()
    {
        var (db, routine) = await BuildWithRoutineAsync();
        var handler = BuildHandler(db);

        await handler.Handle(
            new PauseRoutineCommand(routine.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<Routine>()
            .SingleOrDefaultAsync(r => r.Id == routine.Id);
        saved!.Status.Should().Be(RoutineStatus.Paused);
    }

    [Fact]
    public async Task Handle_RoutineNotFound_ThrowsTasksException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new PauseRoutineCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.RoutineNotFound);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsTasksException()
    {
        var (db, routine) = await BuildWithRoutineAsync();
        var auth = new StubTasksAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new PauseRoutineCommand(routine.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_AlreadyPaused_ThrowsTasksException()
    {
        var (db, routine) = await BuildWithRoutineAsync();
        routine.Pause();
        await db.SaveChangesAsync();
        routine.ClearDomainEvents();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new PauseRoutineCommand(routine.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.RoutineAlreadyPaused);
    }
}
