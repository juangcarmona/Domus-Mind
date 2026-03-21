using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Features.Calendar.DetectCalendarConflicts;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Domain.Shared;
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
        DateOnly startDate,
        TimeOnly startTime,
        DateOnly? endDate = null,
        TimeOnly? endTime = null)
    {
        var eventTime = (endDate.HasValue && endTime.HasValue)
            ? EventTime.Range(startDate, startTime, endDate.Value, endTime.Value)
            : EventTime.Moment(startDate, startTime);
        return Domain.Calendar.CalendarEvent.Create(
            CalendarEventId.New(), familyId,
            EventTitle.Create(title), null,
            eventTime, HexColor.From("#3B82F6"), DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_NoEvents_ReturnsNoConflicts()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new DetectCalendarConflictsQuery(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), null, Guid.NewGuid()),
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
            new DetectCalendarConflictsQuery(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), null, Guid.NewGuid()),
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
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var eventA = MakeEvent(familyId, "Event A", startDate, new TimeOnly(9, 0), startDate, new TimeOnly(11, 0));
        var eventB = MakeEvent(familyId, "Event B", startDate, new TimeOnly(10, 0), startDate, new TimeOnly(12, 0));

        eventA.AddParticipant(participantId);
        eventB.AddParticipant(participantId);

        db.Set<Domain.Calendar.CalendarEvent>().AddRange(eventA, eventB);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new DetectCalendarConflictsQuery(familyId.Value, startDate, startDate.AddDays(1), Guid.NewGuid()),
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
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var eventA = MakeEvent(familyId, "Morning", startDate, new TimeOnly(8, 0), startDate, new TimeOnly(10, 0));
        var eventB = MakeEvent(familyId, "Evening", startDate, new TimeOnly(18, 0), startDate, new TimeOnly(20, 0));

        eventA.AddParticipant(participantId);
        eventB.AddParticipant(participantId);

        db.Set<Domain.Calendar.CalendarEvent>().AddRange(eventA, eventB);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new DetectCalendarConflictsQuery(familyId.Value, startDate, startDate.AddDays(1), Guid.NewGuid()),
            CancellationToken.None);

        result.Conflicts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_TwoOverlappingEventsNoSharedParticipants_NotConflict()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var eventA = MakeEvent(familyId, "Event A", startDate, new TimeOnly(9, 0), startDate, new TimeOnly(11, 0));
        var eventB = MakeEvent(familyId, "Event B", startDate, new TimeOnly(10, 0), startDate, new TimeOnly(12, 0));

        eventA.AddParticipant(MemberId.New());
        eventB.AddParticipant(MemberId.New()); // Different participants

        db.Set<Domain.Calendar.CalendarEvent>().AddRange(eventA, eventB);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new DetectCalendarConflictsQuery(familyId.Value, startDate, startDate.AddDays(1), Guid.NewGuid()),
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
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Both events overlap but belong to different families
        var eventA = MakeEvent(familyA, "Family A", startDate, new TimeOnly(9, 0), startDate, new TimeOnly(11, 0));
        var eventB = MakeEvent(familyB, "Family B", startDate, new TimeOnly(10, 0), startDate, new TimeOnly(12, 0));
        eventA.AddParticipant(participantId);
        eventB.AddParticipant(participantId);

        db.Set<Domain.Calendar.CalendarEvent>().AddRange(eventA, eventB);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new DetectCalendarConflictsQuery(familyA.Value, startDate, startDate.AddDays(1), Guid.NewGuid()),
            CancellationToken.None);

        result.Conflicts.Should().BeEmpty();
    }
}
