using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.GetHouseholdTimeline;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskStatus = DomusMind.Domain.Tasks.TaskStatus;

namespace DomusMind.Application.Tests.Features.Family;

public sealed class GetHouseholdTimelineQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetHouseholdTimelineQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubFamilyTimelineAuthorizationService? auth = null)
        => new(db, auth ?? new StubFamilyTimelineAuthorizationService());

    private static Domain.Calendar.CalendarEvent MakeCalendarEvent(
        FamilyId familyId,
        string title,
        DateTime startTime)
        => Domain.Calendar.CalendarEvent.Create(
            CalendarEventId.New(), familyId,
            EventTitle.Create(title), null,
            startTime, null, DateTime.UtcNow);

    private static HouseholdTask MakeTask(
        FamilyId familyId,
        string title,
        DateTime? dueDate = null)
        => HouseholdTask.Create(
            TaskId.New(), familyId,
            TaskTitle.Create(title), null,
            dueDate, DateTime.UtcNow);

    private static Routine MakeRoutine(
        FamilyId familyId,
        string name)
        => Routine.Create(
            RoutineId.New(), familyId,
            RoutineName.Create(name), "Daily", DateTime.UtcNow);

    // --- Authorization ---

    [Fact]
    public async Task Handle_AccessDenied_ThrowsFamilyException()
    {
        var db = CreateDb();
        var auth = new StubFamilyTimelineAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new GetHouseholdTimelineQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.AccessDenied);
    }

    // --- Empty state ---

    [Fact]
    public async Task Handle_NoEntries_ReturnsEmptyList()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdTimelineQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.Entries.Should().BeEmpty();
    }

    // --- Source inclusion ---

    [Fact]
    public async Task Handle_WithCalendarEvent_IncludesCalendarEventEntry()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var evt = MakeCalendarEvent(familyId, "School Play", DateTime.UtcNow.AddDays(3));
        db.Set<Domain.Calendar.CalendarEvent>().Add(evt);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdTimelineQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Entries.Should().ContainSingle()
            .Which.EntryType.Should().Be("CalendarEvent");
    }

    [Fact]
    public async Task Handle_WithTask_IncludesTaskEntry()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var task = MakeTask(familyId, "Buy milk", DateTime.UtcNow.AddDays(1));
        db.Set<HouseholdTask>().Add(task);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdTimelineQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Entries.Should().ContainSingle()
            .Which.EntryType.Should().Be("Task");
    }

    [Fact]
    public async Task Handle_WithRoutine_IncludesRoutineEntry()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var routine = MakeRoutine(familyId, "Evening Cleanup");
        db.Set<Routine>().Add(routine);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdTimelineQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Entries.Should().ContainSingle()
            .Which.EntryType.Should().Be("Routine");
    }

    // --- Mixed sources ---

    [Fact]
    public async Task Handle_WithMixedSources_ReturnsAllThreeEntryTypes()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        db.Set<Domain.Calendar.CalendarEvent>().Add(
            MakeCalendarEvent(familyId, "Family Dinner", DateTime.UtcNow.AddDays(2)));
        db.Set<HouseholdTask>().Add(
            MakeTask(familyId, "Fix the sink", DateTime.UtcNow.AddDays(1)));
        db.Set<Routine>().Add(
            MakeRoutine(familyId, "Morning Workout"));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdTimelineQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Entries.Should().HaveCount(3);
        result.Entries.Select(e => e.EntryType).Should()
            .Contain("CalendarEvent")
            .And.Contain("Task")
            .And.Contain("Routine");
    }

    // --- Ordering ---

    [Fact]
    public async Task Handle_DatedEntriesOrderedByEffectiveDateAscending()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        db.Set<Domain.Calendar.CalendarEvent>().Add(
            MakeCalendarEvent(familyId, "Later Event", DateTime.UtcNow.AddDays(5)));
        db.Set<HouseholdTask>().Add(
            MakeTask(familyId, "Earlier Task", DateTime.UtcNow.AddDays(1)));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdTimelineQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Entries.Should().HaveCount(2);
        result.Entries.First().Title.Should().Be("Earlier Task");
        result.Entries.Last().Title.Should().Be("Later Event");
    }

    [Fact]
    public async Task Handle_RoutinesAppearAfterDatedEntries()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        db.Set<Domain.Calendar.CalendarEvent>().Add(
            MakeCalendarEvent(familyId, "Dentist Appointment", DateTime.UtcNow.AddDays(3)));
        db.Set<Routine>().Add(
            MakeRoutine(familyId, "Daily Standup"));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdTimelineQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Entries.Should().HaveCount(2);
        result.Entries.First().EntryType.Should().Be("CalendarEvent");
        result.Entries.Last().EntryType.Should().Be("Routine");
        result.Entries.Last().EffectiveDate.Should().BeNull();
    }

    [Fact]
    public async Task Handle_TasksWithNoDueDateAppearAfterDatedEntries()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        db.Set<HouseholdTask>().Add(MakeTask(familyId, "Undated Task", null));
        db.Set<HouseholdTask>().Add(MakeTask(familyId, "Dated Task", DateTime.UtcNow.AddDays(1)));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdTimelineQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Entries.Should().HaveCount(2);
        result.Entries.First().Title.Should().Be("Dated Task");
        result.Entries.Last().Title.Should().Be("Undated Task");
    }

    // --- Family scoping ---

    [Fact]
    public async Task Handle_ExcludesEntriesFromOtherFamily()
    {
        var db = CreateDb();
        var familyA = FamilyId.New();
        var familyB = FamilyId.New();
        db.Set<Domain.Calendar.CalendarEvent>().Add(
            MakeCalendarEvent(familyA, "Family A Event", DateTime.UtcNow.AddDays(1)));
        db.Set<Domain.Calendar.CalendarEvent>().Add(
            MakeCalendarEvent(familyB, "Family B Event", DateTime.UtcNow.AddDays(1)));
        db.Set<HouseholdTask>().Add(MakeTask(familyB, "Family B Task"));
        db.Set<Routine>().Add(MakeRoutine(familyB, "Family B Routine"));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdTimelineQuery(familyA.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Entries.Should().ContainSingle()
            .Which.Title.Should().Be("Family A Event");
    }

    // --- Entry field correctness ---

    [Fact]
    public async Task Handle_CalendarEventEntry_HasCorrectFields()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var startTime = DateTime.UtcNow.AddDays(4);
        var evt = MakeCalendarEvent(familyId, "School Concert", startTime);
        db.Set<Domain.Calendar.CalendarEvent>().Add(evt);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdTimelineQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        var entry = result.Entries.Single();
        entry.EntryId.Should().Be(evt.Id.Value);
        entry.EntryType.Should().Be("CalendarEvent");
        entry.Title.Should().Be("School Concert");
        entry.EffectiveDate.Should().Be(startTime);
        entry.Status.Should().Be("Scheduled");
    }

    [Fact]
    public async Task Handle_TaskEntry_HasCorrectFields()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var dueDate = DateTime.UtcNow.AddDays(2);
        var task = MakeTask(familyId, "Water plants", dueDate);
        db.Set<HouseholdTask>().Add(task);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdTimelineQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        var entry = result.Entries.Single();
        entry.EntryId.Should().Be(task.Id.Value);
        entry.EntryType.Should().Be("Task");
        entry.Title.Should().Be("Water plants");
        entry.EffectiveDate.Should().Be(dueDate);
        entry.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_RoutineEntry_HasCorrectFields()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var routine = MakeRoutine(familyId, "Evening Walk");
        db.Set<Routine>().Add(routine);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdTimelineQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        var entry = result.Entries.Single();
        entry.EntryId.Should().Be(routine.Id.Value);
        entry.EntryType.Should().Be("Routine");
        entry.Title.Should().Be("Evening Walk");
        entry.EffectiveDate.Should().BeNull();
        entry.Status.Should().Be("Active");
    }
}
