using DomusMind.Application.Features.Tasks;
using DomusMind.Application.Features.Tasks.UpdateRoutine;
using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Tasks;

public sealed class UpdateRoutineCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static UpdateRoutineCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubTasksAuthorizationService? auth = null)
        => new(db, new EventLogWriter(db), auth ?? new StubTasksAuthorizationService());

    private static async Task<(DomusMindDbContext Db, Routine Routine)> BuildWithRoutineAsync()
    {
        var db = CreateDb();
        var routine = Routine.Create(
            RoutineId.New(), FamilyId.New(),
            RoutineName.Create("Daily Standup"), "Every day at 09:00", DateTime.UtcNow);
        db.Set<Routine>().Add(routine);
        await db.SaveChangesAsync();
        routine.ClearDomainEvents();
        return (db, routine);
    }

    [Fact]
    public async Task Handle_WithValidInput_ReturnsResponse()
    {
        var (db, routine) = await BuildWithRoutineAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new UpdateRoutineCommand(routine.Id.Value, "Weekly Retro", "Every Friday at 16:00", Guid.NewGuid()),
            CancellationToken.None);

        result.RoutineId.Should().Be(routine.Id.Value);
        result.Name.Should().Be("Weekly Retro");
        result.Cadence.Should().Be("Every Friday at 16:00");
    }

    [Fact]
    public async Task Handle_PersistsUpdatedValues()
    {
        var (db, routine) = await BuildWithRoutineAsync();
        var handler = BuildHandler(db);

        await handler.Handle(
            new UpdateRoutineCommand(routine.Id.Value, "New Name", "Monthly", Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<Routine>()
            .SingleOrDefaultAsync(r => r.Id == routine.Id);
        saved!.Name.Value.Should().Be("New Name");
        saved.Cadence.Should().Be("Monthly");
    }

    [Fact]
    public async Task Handle_RoutineNotFound_ThrowsTasksException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new UpdateRoutineCommand(Guid.NewGuid(), "Name", "Weekly", Guid.NewGuid()),
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
            new UpdateRoutineCommand(routine.Id.Value, "Name", "Weekly", Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.AccessDenied);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyCadence_ThrowsTasksException(string cadence)
    {
        var (db, routine) = await BuildWithRoutineAsync();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new UpdateRoutineCommand(routine.Id.Value, "Valid Name", cadence, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.InvalidInput);
    }
}
