using DomusMind.Application.Features.Tasks;
using DomusMind.Application.Features.Tasks.UpdateRoutine;
using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.Enums;
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

    private static async Task<(DomusMindDbContext Db, Routine Routine, MemberId MemberId)> BuildWithRoutineAsync()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var memberId = MemberId.New();

        var routine = Routine.Create(
            RoutineId.New(),
            familyId,
            RoutineName.Create("Daily Standup"),
            RoutineScope.Members,
            RoutineKind.Scheduled,
            RoutineColor.From("#7C3AED"),
            RoutineSchedule.Weekly(new[] { DayOfWeek.Monday, DayOfWeek.Wednesday }, new TimeOnly(9, 0)),
            new[] { memberId },
            DateTime.UtcNow);

        db.Set<Routine>().Add(routine);
        await db.SaveChangesAsync();
        routine.ClearDomainEvents();

        return (db, routine, memberId);
    }

    [Fact]
    public async Task Handle_WithValidInput_ReturnsResponse()
    {
        var (db, routine, memberId) = await BuildWithRoutineAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new UpdateRoutineCommand(
                routine.Id.Value,
                "Weekly Retro",
                "Members",
                "Scheduled",
                "#22C55E",
                "Weekly",
                new[] { DayOfWeek.Friday },
                Array.Empty<int>(),
                null,
                new TimeOnly(16, 0),
                new[] { memberId.Value },
                Guid.NewGuid()),
            CancellationToken.None);

        result.RoutineId.Should().Be(routine.Id.Value);
        result.Name.Should().Be("Weekly Retro");
        result.Scope.Should().Be("Members");
        result.Kind.Should().Be("Scheduled");
        result.Color.Should().Be("#22C55E");
        result.Frequency.Should().Be("Weekly");
        result.DaysOfWeek.Should().BeEquivalentTo(new[] { DayOfWeek.Friday });
        result.Time.Should().Be(new TimeOnly(16, 0));
        result.TargetMemberIds.Should().Contain(memberId.Value);
    }

    [Fact]
    public async Task Handle_PersistsUpdatedValues()
    {
        var (db, routine, memberId) = await BuildWithRoutineAsync();
        var handler = BuildHandler(db);

        await handler.Handle(
            new UpdateRoutineCommand(
                routine.Id.Value,
                "Review Expenses",
                "Members",
                "Cue",
                "#F59E0B",
                "Monthly",
                Array.Empty<DayOfWeek>(),
                new[] { 1, 15 },
                null,
                null,
                new[] { memberId.Value },
                Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<Routine>()
            .SingleOrDefaultAsync(r => r.Id == routine.Id);

        saved.Should().NotBeNull();
        saved!.Name.Value.Should().Be("Review Expenses");
        saved.Scope.Should().Be(RoutineScope.Members);
        saved.Kind.Should().Be(RoutineKind.Cue);
        saved.Color.Value.Should().Be("#F59E0B");
        saved.Schedule.Frequency.Should().Be(RoutineFrequency.Monthly);
        saved.Schedule.DaysOfMonth.Should().BeEquivalentTo(new[] { 1, 15 });
        saved.TargetMemberIds.Select(x => x.Value).Should().BeEquivalentTo(new[] { memberId.Value });
    }

    [Fact]
    public async Task Handle_RoutineNotFound_ThrowsTasksException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new UpdateRoutineCommand(
                Guid.NewGuid(),
                "Name",
                "Members",
                "Cue",
                "#3B82F6",
                "Weekly",
                new[] { DayOfWeek.Monday },
                Array.Empty<int>(),
                null,
                null,
                new[] { Guid.NewGuid() },
                Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.RoutineNotFound);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsTasksException()
    {
        var (db, routine, memberId) = await BuildWithRoutineAsync();
        var auth = new StubTasksAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new UpdateRoutineCommand(
                routine.Id.Value,
                "Name",
                "Members",
                "Cue",
                "#3B82F6",
                "Weekly",
                new[] { DayOfWeek.Monday },
                Array.Empty<int>(),
                null,
                null,
                new[] { memberId.Value },
                Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.AccessDenied);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyColor_ThrowsTasksException(string color)
    {
        var (db, routine, memberId) = await BuildWithRoutineAsync();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new UpdateRoutineCommand(
                routine.Id.Value,
                "Valid Name",
                "Members",
                "Cue",
                color,
                "Weekly",
                new[] { DayOfWeek.Monday },
                Array.Empty<int>(),
                null,
                null,
                new[] { memberId.Value },
                Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_MemberScopeWithoutTargets_ThrowsTasksException()
    {
        var (db, routine, _) = await BuildWithRoutineAsync();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new UpdateRoutineCommand(
                routine.Id.Value,
                "Valid Name",
                "Members",
                "Cue",
                "#3B82F6",
                "Weekly",
                new[] { DayOfWeek.Monday },
                Array.Empty<int>(),
                null,
                null,
                Array.Empty<Guid>(),
                Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.InvalidInput);
    }
}