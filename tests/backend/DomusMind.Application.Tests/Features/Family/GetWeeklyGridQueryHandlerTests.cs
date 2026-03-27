using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.GetWeeklyGrid;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Domain.Shared;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.Enums;
using DomusMind.Domain.Tasks.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Family;

public sealed class GetWeeklyGridQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetWeeklyGridQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubFamilyTimelineAuthorizationService? auth = null)
        => new(db, auth ?? new StubFamilyTimelineAuthorizationService());

    private static Domain.Family.Family MakeFamily(FamilyId familyId, params (MemberId Id, string Name)[] members)
    {
        var family = Domain.Family.Family.Create(
            familyId, FamilyName.Create("Test Family"), null, DateTime.UtcNow);
        foreach (var (id, name) in members)
            family.AddMember(id, MemberName.Create(name), MemberRole.Create("Adult"), DateTime.UtcNow);
        family.ClearDomainEvents();
        return family;
    }

    private static Domain.Calendar.CalendarEvent MakeEvent(
        FamilyId familyId,
        string title,
        DateOnly date,
        TimeOnly? time = null,
        MemberId? participant = null)
    {
        var eventTime = time.HasValue
            ? EventTime.Moment(date, time.Value)
            : EventTime.Day(date);
        var evt = Domain.Calendar.CalendarEvent.Create(
            CalendarEventId.New(), familyId,
            EventTitle.Create(title), null,
            eventTime,
            HexColor.From("#3B82F6"),
            DateTime.UtcNow);
        if (participant is not null)
            evt.AddParticipant(participant.Value);
        return evt;
    }

    private static HouseholdTask MakeTask(
        FamilyId familyId,
        string title,
        DateOnly? dueDate = null,
        MemberId? assignee = null)
    {
        var schedule = dueDate.HasValue
            ? TaskSchedule.WithDueDate(dueDate.Value)
            : TaskSchedule.NoSchedule();
        var task = HouseholdTask.Create(
            TaskId.New(), familyId,
            TaskTitle.Create(title), null,
            schedule,
            HexColor.From("#3B82F6"),
            DateTime.UtcNow);
        if (assignee is not null)
            task.Assign(assignee.Value);
        return task;
    }

    private static Routine MakeRoutine(FamilyId familyId, string name)
        => Routine.Create(
            RoutineId.New(), familyId,
            RoutineName.Create(name),
            RoutineScope.Household,
            RoutineKind.Cue,
            HexColor.From("#3B82F6"),
            RoutineSchedule.Weekly(
                new[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday,
                         DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday }),
            null,
            DateTime.UtcNow);

    // ---- Authorization / guarding ----

    [Fact]
    public async Task Handle_AccessDenied_ThrowsFamilyException()
    {
        var db = CreateDb();
        var auth = new StubFamilyTimelineAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new GetWeeklyGridQuery(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_FamilyNotFound_ThrowsFamilyException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new GetWeeklyGridQuery(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.FamilyNotFound);
    }

    // ---- UTC normalization ----

    [Fact]
    public async Task Handle_DateOnlyWeekStart_ReturnsCorrectWeekStartString()
    {
        // DateOnly has no Kind concept; verify the handler returns the ISO date string correctly
        var weekStart = new DateOnly(2026, 3, 16);

        var db = CreateDb();
        var familyId = FamilyId.New();
        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetWeeklyGridQuery(familyId.Value, weekStart, Guid.NewGuid()),
            CancellationToken.None);

        result.WeekStart.Should().Be("2026-03-16");
        result.WeekEnd.Should().Be("2026-03-22");
    }

    [Fact]
    public async Task Handle_WeekStartDateOnly_ReturnsMatchingWeekStartInResponse()
    {
        var weekStart = new DateOnly(2026, 3, 16);
        var db = CreateDb();
        var familyId = FamilyId.New();
        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetWeeklyGridQuery(familyId.Value, weekStart, Guid.NewGuid()),
            CancellationToken.None);

        result.WeekStart.Should().Be("2026-03-16");
        result.WeekEnd.Should().Be("2026-03-22");
    }

    // ---- Omitted / defaulted weekStart ----

    [Fact]
    public async Task Handle_WhenWeekStartIsCurrentWeek_ReturnsSevenDayCells()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var memberId = MemberId.New();
        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, (memberId, "Alice")));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var weekStart = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await handler.Handle(
            new GetWeeklyGridQuery(familyId.Value, weekStart, Guid.NewGuid()),
            CancellationToken.None);

        result.Members.Should().ContainSingle();
        result.Members.First().Cells.Should().HaveCount(7);
    }

    // ---- Event inclusion in range ----

    [Fact]
    public async Task Handle_EventInsideWeekWindow_AppearsInMemberCell()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var memberId = MemberId.New();
        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, (memberId, "Bob")));

        var weekStart = new DateOnly(2026, 3, 16);
        var eventDate = weekStart.AddDays(2); // Wednesday
        db.Set<Domain.Calendar.CalendarEvent>().Add(MakeEvent(familyId, "School Run", eventDate, new TimeOnly(9, 0), memberId));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetWeeklyGridQuery(familyId.Value, weekStart, Guid.NewGuid()),
            CancellationToken.None);

        var wednesdayCell = result.Members.First().Cells.Single(c => c.Date == eventDate.ToString("yyyy-MM-dd"));
        wednesdayCell.Events.Should().ContainSingle(e => e.Title == "School Run");
    }

    [Fact]
    public async Task Handle_EventOutsideWeekWindow_IsExcluded()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var memberId = MemberId.New();
        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, (memberId, "Carol")));

        var weekStart = new DateOnly(2026, 3, 16);
        var outsideEvent = weekStart.AddDays(8); // beyond the window
        db.Set<Domain.Calendar.CalendarEvent>().Add(MakeEvent(familyId, "Future Event", outsideEvent, null, memberId));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetWeeklyGridQuery(familyId.Value, weekStart, Guid.NewGuid()),
            CancellationToken.None);

        result.Members.First().Cells.SelectMany(c => c.Events).Should().BeEmpty();
    }

    // ---- Task inclusion in range ----

    [Fact]
    public async Task Handle_TaskDueInsideWeekWindow_AppearsInMemberCell()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var memberId = MemberId.New();
        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, (memberId, "Dan")));

        var weekStart = new DateOnly(2026, 3, 16);
        var dueDate = weekStart.AddDays(1); // Tuesday
        db.Set<HouseholdTask>().Add(MakeTask(familyId, "Take out bins", dueDate, memberId));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetWeeklyGridQuery(familyId.Value, weekStart, Guid.NewGuid()),
            CancellationToken.None);

        var tuesdayCell = result.Members.First().Cells.Single(c => c.Date == dueDate.ToString("yyyy-MM-dd"));
        tuesdayCell.Tasks.Should().ContainSingle(t => t.Title == "Take out bins");
    }

    [Fact]
    public async Task Handle_TaskDueOutsideWeekWindow_IsExcluded()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var memberId = MemberId.New();
        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, (memberId, "Eve")));

        var weekStart = new DateOnly(2026, 3, 16);
        var pastTask = weekStart.AddDays(-1); // before the window
        db.Set<HouseholdTask>().Add(MakeTask(familyId, "Old task", pastTask, memberId));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetWeeklyGridQuery(familyId.Value, weekStart, Guid.NewGuid()),
            CancellationToken.None);

        result.Members.First().Cells.SelectMany(c => c.Tasks).Should().BeEmpty();
    }

    // ---- Routines ----

    [Fact]
    public async Task Handle_ActiveRoutine_AppearsInCells()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var memberId = MemberId.New();
        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, (memberId, "Alice")));
        db.Set<Routine>().Add(MakeRoutine(familyId, "Morning Walk"));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetWeeklyGridQuery(familyId.Value, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid()),
            CancellationToken.None);

        result.SharedCells.SelectMany(c => c.Routines).Should().Contain(r => r.Name == "Morning Walk");
        result.Members.First().Cells.SelectMany(c => c.Routines).Should().BeEmpty();
    }

    // ---- Response shape ----

    [Fact]
    public async Task Handle_WeekBoundaries_AreCorrect()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var weekStart = new DateOnly(2026, 3, 16);
        var result = await handler.Handle(
            new GetWeeklyGridQuery(familyId.Value, weekStart, Guid.NewGuid()),
            CancellationToken.None);

        result.WeekStart.Should().Be("2026-03-16");
        result.WeekEnd.Should().Be("2026-03-22");
    }

    [Fact]
    public async Task Handle_EmptyFamily_ReturnsMemberRowsWithEmptyCells()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var memberId = MemberId.New();
        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, (memberId, "Fred")));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetWeeklyGridQuery(familyId.Value, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid()),
            CancellationToken.None);

        result.Members.Should().ContainSingle(m => m.Name == "Fred");
        result.Members.First().Cells.Should().AllSatisfy(c =>
        {
            c.Events.Should().BeEmpty();
            c.Tasks.Should().BeEmpty();
        });
    }

    // ---- Routine per-cell projection ----

    [Fact]
    public async Task Handle_ActiveRoutineOnSpecificDay_AppearsOnlyInMatchingCells()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var memberId = MemberId.New();
        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, (memberId, "Alice")));

        // Member-scoped routine that runs only on Wednesday
        var wednesdayRoutine = Routine.Create(
            RoutineId.New(), familyId,
            RoutineName.Create("Wednesday Task"),
            RoutineScope.Members,
            RoutineKind.Scheduled,
            HexColor.From("#FF0000"),
            RoutineSchedule.Weekly(new[] { DayOfWeek.Wednesday }),
            new[] { memberId },
            DateTime.UtcNow);
        db.Set<Routine>().Add(wednesdayRoutine);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        // Week of Monday 16 March 2026 - Wednesday is 18 March
        var weekStart = new DateOnly(2026, 3, 16);
        var result = await handler.Handle(
            new GetWeeklyGridQuery(familyId.Value, weekStart, Guid.NewGuid()),
            CancellationToken.None);

        var cells = result.Members.First().Cells;
        var wednesdayCell = cells.Single(c => DateOnly.ParseExact(c.Date, "yyyy-MM-dd").DayOfWeek == DayOfWeek.Wednesday);
        wednesdayCell.Routines.Should().ContainSingle(r => r.Name == "Wednesday Task");

        // All other days must be empty
        cells.Where(c => DateOnly.ParseExact(c.Date, "yyyy-MM-dd").DayOfWeek != DayOfWeek.Wednesday)
             .Should().AllSatisfy(c => c.Routines.Should().BeEmpty());
    }

    [Fact]
    public async Task Handle_NoRoutines_AllCellsHaveEmptyRoutinesCollection()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var memberId = MemberId.New();
        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, (memberId, "Bob")));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetWeeklyGridQuery(familyId.Value, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid()),
            CancellationToken.None);

        result.Members.First().Cells.Should().AllSatisfy(c =>
            c.Routines.Should().NotBeNull().And.BeEmpty());
    }

    [Fact]
    public async Task Handle_RoutineContractFields_AllPresentInProjection()
    {
        // Regression: old types had cadence/status; new contract exposes kind/color/frequency/time/scope
        var db = CreateDb();
        var familyId = FamilyId.New();
        var memberId = MemberId.New();
        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, (memberId, "Carol")));

        var routine = Routine.Create(
            RoutineId.New(), familyId,
            RoutineName.Create("Morning Walk"),
            RoutineScope.Household,
            RoutineKind.Scheduled,
            HexColor.From("#3B82F6"),
            RoutineSchedule.Weekly(new[] { DayOfWeek.Monday }),
            null,
            DateTime.UtcNow);
        db.Set<Routine>().Add(routine);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var weekStart = new DateOnly(2026, 3, 16); // Monday
        var result = await handler.Handle(
            new GetWeeklyGridQuery(familyId.Value, weekStart, Guid.NewGuid()),
            CancellationToken.None);

        // Household routine goes to SharedCells, not member cells
        var mondaySharedCell = result.SharedCells.Single(c => DateOnly.ParseExact(c.Date, "yyyy-MM-dd").DayOfWeek == DayOfWeek.Monday);
        var item = mondaySharedCell.Routines.Should().ContainSingle().Which;

        item.Name.Should().Be("Morning Walk");
        item.Kind.Should().NotBeNullOrEmpty();    // e.g. "Scheduled"
        item.Color.Should().Be("#3B82F6");
        item.Frequency.Should().NotBeNullOrEmpty(); // e.g. "Weekly"
        item.Scope.Should().NotBeNullOrEmpty();     // e.g. "Household"
    }

    [Fact]
    public async Task Handle_HouseholdRoutine_AppearsInSharedCellsAndNotInMemberCells()
    {
        // Regression: household routines must not be duplicated into every member row.
        // They must appear once in SharedCells and not at all in member cells.
        var db = CreateDb();
        var familyId = FamilyId.New();
        var member1 = MemberId.New();
        var member2 = MemberId.New();
        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, (member1, "Alice"), (member2, "Bob")));

        var householdRoutine = Routine.Create(
            RoutineId.New(), familyId,
            RoutineName.Create("Carmen"),
            RoutineScope.Household,
            RoutineKind.Scheduled,
            HexColor.From("#FF8800"),
            RoutineSchedule.Weekly(new[] { DayOfWeek.Monday, DayOfWeek.Thursday },
                new TimeOnly(8, 30)),
            null,
            DateTime.UtcNow);
        db.Set<Routine>().Add(householdRoutine);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var weekStart = new DateOnly(2026, 3, 16); // Week: Mon 16 – Sun 22 March
        var result = await handler.Handle(
            new GetWeeklyGridQuery(familyId.Value, weekStart, Guid.NewGuid()),
            CancellationToken.None);

        // SharedCells must have Carmen on Monday and Thursday
        result.SharedCells.Single(c => DateOnly.ParseExact(c.Date, "yyyy-MM-dd").DayOfWeek == DayOfWeek.Monday)
              .Routines.Should().ContainSingle(r => r.Name == "Carmen");
        result.SharedCells.Single(c => DateOnly.ParseExact(c.Date, "yyyy-MM-dd").DayOfWeek == DayOfWeek.Thursday)
              .Routines.Should().ContainSingle(r => r.Name == "Carmen");

        // No member cell may contain Carmen
        result.Members.Should().AllSatisfy(m =>
            m.Cells.Should().AllSatisfy(c =>
                c.Routines.Should().NotContain(r => r.Name == "Carmen")));
    }

    // ---- Member ordering ----

    [Fact]
    public async Task Handle_MembersWithBirthDates_OrderedByBirthDateAscendingThenByName()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var family = Domain.Family.Family.Create(
            familyId, FamilyName.Create("Test Family"), null, DateTime.UtcNow);

        // Add in reverse expected order to confirm sorting is applied
        var olderMemberId = MemberId.New();
        family.AddMember(olderMemberId, MemberName.Create("Zara"), MemberRole.Create("Adult"),
            false, new DateOnly(1985, 6, 15), DateTime.UtcNow);

        var youngerMemberId = MemberId.New();
        family.AddMember(youngerMemberId, MemberName.Create("Alice"), MemberRole.Create("Adult"),
            false, new DateOnly(2000, 3, 1), DateTime.UtcNow);

        // No birthdate - falls after dated members, sorted alphabetically
        var noDobB = MemberId.New();
        family.AddMember(noDobB, MemberName.Create("Bob"), MemberRole.Create("Adult"),
            false, null, DateTime.UtcNow);

        var noDobA = MemberId.New();
        family.AddMember(noDobA, MemberName.Create("Anna"), MemberRole.Create("Adult"),
            false, null, DateTime.UtcNow);

        family.ClearDomainEvents();
        db.Set<Domain.Family.Family>().Add(family);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var weekStart = new DateOnly(2026, 3, 16);
        var result = await handler.Handle(
            new GetWeeklyGridQuery(familyId.Value, weekStart, Guid.NewGuid()),
            CancellationToken.None);

        var names = result.Members.Select(m => m.Name).ToList();
        names.Should().Equal("Zara", "Alice", "Anna", "Bob");
    }

    [Fact]
    public async Task Handle_MembersWithoutBirthDates_OrderedAlphabetically()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var family = Domain.Family.Family.Create(
            familyId, FamilyName.Create("Test Family"), null, DateTime.UtcNow);

        family.AddMember(MemberId.New(), MemberName.Create("Zoe"), MemberRole.Create("Adult"),
            false, null, DateTime.UtcNow);
        family.AddMember(MemberId.New(), MemberName.Create("Aaron"), MemberRole.Create("Adult"),
            false, null, DateTime.UtcNow);
        family.AddMember(MemberId.New(), MemberName.Create("Mike"), MemberRole.Create("Adult"),
            false, null, DateTime.UtcNow);

        family.ClearDomainEvents();
        db.Set<Domain.Family.Family>().Add(family);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var weekStart = new DateOnly(2026, 3, 16);
        var result = await handler.Handle(
            new GetWeeklyGridQuery(familyId.Value, weekStart, Guid.NewGuid()),
            CancellationToken.None);

        var names = result.Members.Select(m => m.Name).ToList();
        names.Should().Equal("Aaron", "Mike", "Zoe");
    }
}