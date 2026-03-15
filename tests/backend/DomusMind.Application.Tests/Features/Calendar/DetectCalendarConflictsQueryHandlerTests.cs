using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Features.Calendar.DetectCalendarConflicts;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Calendar;

public sealed class DetectCalendarConflictsQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static DetectCalendarConflictsQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubCalendarAuthorizationService? auth = null)
        => new(db, auth ?? new StubCalendarAuthorizationService());

    private static Domain.Calendar.CalendarEvent MakeEvent(
        FamilyId familyId,
        string title,
        DateTime startTime,
        DateTime? endTime = null)
        => Domain.Calendar.CalendarEvent.Create(
            CalendarEventId.New(), familyId,
            EventTitle.Create(title), null,
            startTime, endTime, DateTime.UtcNow);

    [Fact]
    public async Task Handle_NoEvents_ReturnsNoConflicts()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new DetectCalendarConflictsQuery(Guid.NewGuid(), DateTime.UtcNow, null, Guid.NewGuid()),
            CancellationToken.None);

        result.Conflicts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsCalendarException()
    {
        var db = CreateDb();
        var auth = new StubCalendarAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new DetectCalendarConflictsQuery(Guid.NewGuid(), DateTime.UtcNow, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_TwoOverlappingEventsWithSharedParticipant_ReturnsConflict()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var participantId = MemberId.New();
        var start = DateTime.UtcNow.AddDays(1);

        var eventA = MakeEvent(familyId, "Event A", start, start.AddHours(2));
        var eventB = MakeEvent(familyId, "Event B", start.AddHours(1), start.AddHours(3));

        eventA.AddParticipant(participantId);
        eventB.AddParticipant(participantId);

        db.Set<Domain.Calendar.CalendarEvent>().AddRange(eventA, eventB);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new DetectCalendarConflictsQuery(familyId.Value, start.AddHours(-1), start.AddHours(4), Guid.NewGuid()),
            CancellationToken.None);

        result.Conflicts.Should().HaveCount(1);
        result.Conflicts.Single().SharedParticipantIds.Should().Contain(participantId.Value);
    }

    [Fact]
    public async Task Handle_TwoNonOverlappingEvents_ReturnsNoConflicts()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var participantId = MemberId.New();
        var start = DateTime.UtcNow.AddDays(1);

        var eventA = MakeEvent(familyId, "Morning", start.AddHours(8), start.AddHours(10));
        var eventB = MakeEvent(familyId, "Evening", start.AddHours(18), start.AddHours(20));

        eventA.AddParticipant(participantId);
        eventB.AddParticipant(participantId);

        db.Set<Domain.Calendar.CalendarEvent>().AddRange(eventA, eventB);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new DetectCalendarConflictsQuery(familyId.Value, start, start.AddDays(1), Guid.NewGuid()),
            CancellationToken.None);

        result.Conflicts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_TwoOverlappingEventsNoSharedParticipants_NotConflict()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var start = DateTime.UtcNow.AddDays(1);

        var eventA = MakeEvent(familyId, "Event A", start, start.AddHours(2));
        var eventB = MakeEvent(familyId, "Event B", start.AddHours(1), start.AddHours(3));

        eventA.AddParticipant(MemberId.New());
        eventB.AddParticipant(MemberId.New()); // Different participants

        db.Set<Domain.Calendar.CalendarEvent>().AddRange(eventA, eventB);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new DetectCalendarConflictsQuery(familyId.Value, start.AddHours(-1), start.AddHours(4), Guid.NewGuid()),
            CancellationToken.None);

        result.Conflicts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ExcludesOtherFamilyEvents()
    {
        var db = CreateDb();
        var familyA = FamilyId.New();
        var familyB = FamilyId.New();
        var participantId = MemberId.New();
        var start = DateTime.UtcNow.AddDays(1);

        // Both events overlap but belong to different families
        var eventA = MakeEvent(familyA, "Family A", start, start.AddHours(2));
        var eventB = MakeEvent(familyB, "Family B", start.AddHours(1), start.AddHours(3));
        eventA.AddParticipant(participantId);
        eventB.AddParticipant(participantId);

        db.Set<Domain.Calendar.CalendarEvent>().AddRange(eventA, eventB);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new DetectCalendarConflictsQuery(familyA.Value, start.AddHours(-1), start.AddHours(4), Guid.NewGuid()),
            CancellationToken.None);

        result.Conflicts.Should().BeEmpty();
    }
}
