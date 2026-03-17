using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.GetEnrichedTimeline;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.Enums;
using DomusMind.Domain.Tasks.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Family;

public sealed class GetEnrichedTimelineQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetEnrichedTimelineQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubFamilyTimelineAuthorizationService? auth = null)
        => new(db, auth ?? new StubFamilyTimelineAuthorizationService());

    private static GetEnrichedTimelineQuery NoFilter(Guid familyId)
        => new(familyId, null, null, null, null, null, Guid.NewGuid());

    [Fact]
    public async Task Handle_AccessDenied_ThrowsFamilyException()
    {
        var db = CreateDb();
        var auth = new StubFamilyTimelineAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(NoFilter(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_NoEntries_ReturnsEmptyGroups()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);
        var familyId = FamilyId.New();

        var result = await handler.Handle(NoFilter(familyId.Value), CancellationToken.None);

        result.Groups.Should().BeEmpty();
        result.TotalEntries.Should().Be(0);
    }

    [Fact]
    public async Task Handle_OverdueTask_IsInOverdueGroupWithHighPriority()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var task = HouseholdTask.Create(
            TaskId.New(), familyId,
            TaskTitle.Create("Old Task"), null,
            DateTime.UtcNow.AddDays(-2), DateTime.UtcNow);
        task.ClearDomainEvents();
        db.Set<HouseholdTask>().Add(task);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(NoFilter(familyId.Value), CancellationToken.None);

        var group = result.Groups.Should().ContainSingle(g => g.GroupKey == "Overdue").Subject;
        group.Entries.Should().ContainSingle()
            .Which.Priority.Should().Be("High");
    }

    [Fact]
    public async Task Handle_TodayCalendarEvent_IsInTodayGroupWithHighPriority()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var today = DateTime.UtcNow.Date.AddHours(14);
        var evt = CalendarEvent.Create(
            CalendarEventId.New(), familyId,
            EventTitle.Create("Morning Standup"), null,
            today, today.AddHours(1), DateTime.UtcNow);
        db.Set<CalendarEvent>().Add(evt);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(NoFilter(familyId.Value), CancellationToken.None);

        var group = result.Groups.Should().ContainSingle(g => g.GroupKey == "Today").Subject;
        group.Entries.Should().ContainSingle()
            .Which.Priority.Should().Be("High");
    }

    [Fact]
    public async Task Handle_FutureTask_IsInLaterGroup()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var task = HouseholdTask.Create(
            TaskId.New(), familyId,
            TaskTitle.Create("Future Task"), null,
            DateTime.UtcNow.AddDays(30), DateTime.UtcNow);
        task.ClearDomainEvents();
        db.Set<HouseholdTask>().Add(task);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(NoFilter(familyId.Value), CancellationToken.None);

        result.Groups.Should().ContainSingle(g => g.GroupKey == "Later");
    }

    [Fact]
    public async Task Handle_Routine_IsInUndatedGroupWithLowPriority()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();

        var routine = Routine.Create(
            RoutineId.New(),
            familyId,
            RoutineName.Create("Evening Clean"),
            RoutineScope.Household,
            RoutineKind.Cue,
            RoutineColor.From("#7C3AED"),
            RoutineSchedule.Weekly(new[] { DayOfWeek.Monday }),
            Array.Empty<MemberId>(),
            DateTime.UtcNow);

        routine.ClearDomainEvents();

        db.Set<Routine>().Add(routine);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);

        var result = await handler.Handle(NoFilter(familyId.Value), CancellationToken.None);

        var group = result.Groups.Should().ContainSingle(g => g.GroupKey == "Undated").Subject;
        var entry = group.Entries.Should().ContainSingle().Subject;
        entry.EntryType.Should().Be("Routine");
        entry.Priority.Should().Be("Low");
    }

    [Fact]
    public async Task Handle_TypeFilter_ExcludesNonMatchingEntries()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();

        var task = HouseholdTask.Create(
            TaskId.New(),
            familyId,
            TaskTitle.Create("A Task"),
            null,
            DateTime.UtcNow.AddDays(5),
            DateTime.UtcNow);

        task.ClearDomainEvents();

        var routine = Routine.Create(
            RoutineId.New(),
            familyId,
            RoutineName.Create("A Routine"),
            RoutineScope.Household,
            RoutineKind.Cue,
            RoutineColor.From("#0EA5E9"),
            RoutineSchedule.Weekly(new[] { DayOfWeek.Friday }),
            Array.Empty<MemberId>(),
            DateTime.UtcNow);

        routine.ClearDomainEvents();

        db.Set<HouseholdTask>().Add(task);
        db.Set<Routine>().Add(routine);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);

        var query = new GetEnrichedTimelineQuery(
            familyId.Value,
            TypeFilter: new[] { "Task" },
            null, null, null, null, Guid.NewGuid());

        var result = await handler.Handle(query, CancellationToken.None);

        result.Groups.SelectMany(g => g.Entries).Should().OnlyContain(e => e.EntryType == "Task");
    }

    [Fact]
    public async Task Handle_TotalEntries_MatchesAllFilteredEntries()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();

        for (var i = 0; i < 3; i++)
        {
            var task = HouseholdTask.Create(
                TaskId.New(), familyId,
                TaskTitle.Create($"Task {i}"), null,
                DateTime.UtcNow.AddDays(i + 1), DateTime.UtcNow);
            task.ClearDomainEvents();
            db.Set<HouseholdTask>().Add(task);
        }
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(NoFilter(familyId.Value), CancellationToken.None);

        result.TotalEntries.Should().Be(3);
        result.Groups.Sum(g => g.Entries.Count).Should().Be(3);
    }
}
