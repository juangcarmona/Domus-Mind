using DomusMind.Application.Features.Tasks;
using DomusMind.Application.Features.Tasks.CreateRoutine;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.Enums;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Tasks;

public sealed class CreateRoutineCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static CreateRoutineCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubTasksAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new CreateRoutineCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubTasksAuthorizationService());
    }

    private static CreateRoutineCommand WeeklyHouseholdCommand(
        string name = "Morning Workout",
        Guid? familyId = null)
        => new(
            name,
            familyId ?? Guid.NewGuid(),
            "Household",
            "Scheduled",
            "#3B82F6",
            "Weekly",
            new[] { DayOfWeek.Monday, DayOfWeek.Wednesday },
            Array.Empty<int>(),
            null,
            new TimeOnly(7, 0),
            null,
            Array.Empty<Guid>(),
            Guid.NewGuid());

    [Fact]
    public async Task Handle_WithValidInput_ReturnsResponse()
    {
        var handler = BuildHandler();
        var familyId = Guid.NewGuid();

        var result = await handler.Handle(
            WeeklyHouseholdCommand("Morning Workout", familyId),
            CancellationToken.None);

        result.RoutineId.Should().NotBeEmpty();
        result.FamilyId.Should().Be(familyId);
        result.Name.Should().Be("Morning Workout");
        result.Scope.Should().Be("Household");
        result.Kind.Should().Be("Scheduled");
        result.Color.Should().Be("#3B82F6");
        result.Frequency.Should().Be("Weekly");
        result.DaysOfWeek.Should().BeEquivalentTo(new[] { DayOfWeek.Monday, DayOfWeek.Wednesday });
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_PersistsRoutineToDatabase()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            WeeklyHouseholdCommand("Evening Walk"),
            CancellationToken.None);

        var saved = await db.Set<Routine>()
            .SingleOrDefaultAsync(r => r.Id == RoutineId.From(result.RoutineId));
        saved.Should().NotBeNull();
        saved!.Name.Value.Should().Be("Evening Walk");
        saved.Scope.Should().Be(RoutineScope.Household);
        saved.Kind.Should().Be(RoutineKind.Scheduled);
        saved.Schedule.Frequency.Should().Be(RoutineFrequency.Weekly);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyName_ThrowsTasksException(string name)
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            WeeklyHouseholdCommand(name),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_MemberScopedWithNoTargetMembers_ThrowsTasksException()
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new CreateRoutineCommand(
                "Chore Rotation",
                Guid.NewGuid(),
                "Members",
                "Cue",
                "#8B5CF6",
                "Weekly",
                new[] { DayOfWeek.Friday },
                Array.Empty<int>(),
                null,
                null,
                null,
                Array.Empty<Guid>(),
                Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsTasksException()
    {
        var auth = new StubTasksAuthorizationService { CanAccess = false };
        var handler = BuildHandler(auth: auth);

        var act = () => handler.Handle(
            WeeklyHouseholdCommand("Weekend Clean"),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_DailyFrequency_CreatesRoutineWithDailySchedule()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new CreateRoutineCommand(
                "Morning Stretch",
                Guid.NewGuid(),
                "Household",
                "Scheduled",
                "#10B981",
                "Daily",
                Array.Empty<DayOfWeek>(),
                Array.Empty<int>(),
                null,
                null,
                null,
                Array.Empty<Guid>(),
                Guid.NewGuid()),
            CancellationToken.None);

        result.Frequency.Should().Be("Daily");
        result.DaysOfWeek.Should().BeEmpty();
        result.DaysOfMonth.Should().BeEmpty();

        var saved = await db.Set<Routine>()
            .SingleOrDefaultAsync(r => r.Id == RoutineId.From(result.RoutineId));
        saved!.Schedule.Frequency.Should().Be(RoutineFrequency.Daily);
    }

    [Fact]
    public async Task Handle_WithStartAndEndTime_CreatesRoutineAndReturnsEndTime()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new CreateRoutineCommand(
                "Morning Block",
                Guid.NewGuid(),
                "Household",
                "Scheduled",
                "#3B82F6",
                "Daily",
                Array.Empty<DayOfWeek>(),
                Array.Empty<int>(),
                null,
                new TimeOnly(7, 0),
                new TimeOnly(8, 0),
                Array.Empty<Guid>(),
                Guid.NewGuid()),
            CancellationToken.None);

        result.Time.Should().Be(new TimeOnly(7, 0));
        result.EndTime.Should().Be(new TimeOnly(8, 0));

        var saved = await db.Set<Routine>()
            .SingleOrDefaultAsync(r => r.Id == RoutineId.From(result.RoutineId));
        saved!.Schedule.Time.Should().Be(new TimeOnly(7, 0));
        saved.Schedule.EndTime.Should().Be(new TimeOnly(8, 0));
    }

    [Fact]
    public async Task Handle_WithStartTimeOnly_EndTimeIsNull()
    {
        var handler = BuildHandler();

        var result = await handler.Handle(
            new CreateRoutineCommand(
                "Morning Walk",
                Guid.NewGuid(),
                "Household",
                "Scheduled",
                "#3B82F6",
                "Daily",
                Array.Empty<DayOfWeek>(),
                Array.Empty<int>(),
                null,
                new TimeOnly(7, 0),
                null,
                Array.Empty<Guid>(),
                Guid.NewGuid()),
            CancellationToken.None);

        result.Time.Should().Be(new TimeOnly(7, 0));
        result.EndTime.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithoutTime_TimeAndEndTimeAreNull()
    {
        var handler = BuildHandler();

        var result = await handler.Handle(
            new CreateRoutineCommand(
                "Untimed Chore",
                Guid.NewGuid(),
                "Household",
                "Scheduled",
                "#3B82F6",
                "Daily",
                Array.Empty<DayOfWeek>(),
                Array.Empty<int>(),
                null,
                null,
                null,
                Array.Empty<Guid>(),
                Guid.NewGuid()),
            CancellationToken.None);

        result.Time.Should().BeNull();
        result.EndTime.Should().BeNull();
    }

    [Fact]
    public async Task Handle_EndTimeBeforeStartTime_ThrowsTasksException()
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new CreateRoutineCommand(
                "Invalid Block",
                Guid.NewGuid(),
                "Household",
                "Scheduled",
                "#3B82F6",
                "Daily",
                Array.Empty<DayOfWeek>(),
                Array.Empty<int>(),
                null,
                new TimeOnly(10, 0),
                new TimeOnly(9, 0),
                Array.Empty<Guid>(),
                Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_EndTimeEqualToStartTime_ThrowsTasksException()
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new CreateRoutineCommand(
                "Zero Duration",
                Guid.NewGuid(),
                "Household",
                "Scheduled",
                "#3B82F6",
                "Daily",
                Array.Empty<DayOfWeek>(),
                Array.Empty<int>(),
                null,
                new TimeOnly(10, 0),
                new TimeOnly(10, 0),
                Array.Empty<Guid>(),
                Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_EndTimeWithoutStartTime_ThrowsTasksException()
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new CreateRoutineCommand(
                "Dangling EndTime",
                Guid.NewGuid(),
                "Household",
                "Scheduled",
                "#3B82F6",
                "Daily",
                Array.Empty<DayOfWeek>(),
                Array.Empty<int>(),
                null,
                null,
                new TimeOnly(10, 0),
                Array.Empty<Guid>(),
                Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.InvalidInput);
    }
}
