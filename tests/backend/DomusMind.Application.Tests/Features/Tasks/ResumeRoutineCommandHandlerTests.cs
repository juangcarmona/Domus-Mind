using DomusMind.Application.Features.Tasks;
using DomusMind.Application.Features.Tasks.ResumeRoutine;
using DomusMind.Domain.Family;
using DomusMind.Domain.Shared;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.Enums;
using DomusMind.Domain.Tasks.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Tasks;

public sealed class ResumeRoutineCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ResumeRoutineCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubTasksAuthorizationService? auth = null)
        => new(db, new EventLogWriter(db), auth ?? new StubTasksAuthorizationService());

    private static Routine CreateRoutine(
        string name = "Homework Review",
        RoutineScope scope = RoutineScope.Household,
        RoutineKind kind = RoutineKind.Cue)
    {
        return Routine.Create(
            RoutineId.New(),
            FamilyId.New(),
            RoutineName.Create(name),
            scope,
            kind,
            HexColor.From("#8B5CF6"),
            RoutineSchedule.Weekly(
                new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                new TimeOnly(17, 0)),
            targetMembers: Array.Empty<MemberId>(),
            createdAtUtc: DateTime.UtcNow);
    }

    private static async Task<(DomusMindDbContext Db, Routine Routine)> BuildWithPausedRoutineAsync()
    {
        var db = CreateDb();
        var routine = CreateRoutine();
        routine.Pause();

        db.Set<Routine>().Add(routine);
        await db.SaveChangesAsync();
        routine.ClearDomainEvents();

        return (db, routine);
    }

    [Fact]
    public async Task Handle_WithPausedRoutine_ReturnsActiveStatus()
    {
        var (db, routine) = await BuildWithPausedRoutineAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new ResumeRoutineCommand(routine.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.RoutineId.Should().Be(routine.Id.Value);
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_PersistsActiveStatus()
    {
        var (db, routine) = await BuildWithPausedRoutineAsync();
        var handler = BuildHandler(db);

        await handler.Handle(
            new ResumeRoutineCommand(routine.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<Routine>()
            .SingleOrDefaultAsync(r => r.Id == routine.Id);

        saved.Should().NotBeNull();
        saved!.Status.Should().Be(RoutineStatus.Active);
    }

    [Fact]
    public async Task Handle_RoutineNotFound_ThrowsTasksException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new ResumeRoutineCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.RoutineNotFound);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsTasksException()
    {
        var (db, routine) = await BuildWithPausedRoutineAsync();
        var auth = new StubTasksAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new ResumeRoutineCommand(routine.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_AlreadyActive_ThrowsTasksException()
    {
        var db = CreateDb();
        var routine = CreateRoutine(name: "Active Routine");

        db.Set<Routine>().Add(routine);
        await db.SaveChangesAsync();
        routine.ClearDomainEvents();

        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new ResumeRoutineCommand(routine.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.RoutineAlreadyActive);
    }
}