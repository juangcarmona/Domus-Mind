using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Features.Calendar.ProposeAlternativeTimes;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Calendar;

public sealed class ProposeAlternativeTimesQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ProposeAlternativeTimesQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubCalendarAuthorizationService? auth = null)
        => new(db, auth ?? new StubCalendarAuthorizationService());

    [Fact]
    public async Task Handle_AccessDenied_ThrowsCalendarException()
    {
        var db = CreateDb();
        var auth = new StubCalendarAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new ProposeAlternativeTimesQuery(Guid.NewGuid(), Guid.NewGuid(), 3, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_EventNotFound_ThrowsCalendarException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new ProposeAlternativeTimesQuery(Guid.NewGuid(), Guid.NewGuid(), 3, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.EventNotFound);
    }

    [Fact]
    public async Task Handle_DateOnlyEvent_ReturnsEmptySuggestions()
    {
        // Date-only events have no time, so no time-based alternatives can be proposed
        var db = CreateDb();
        var familyId = FamilyId.New();
        var evt = CalendarTestHelpers.MakeEvent(familyId, "All Day Event", new DateOnly(2026, 4, 1));
        db.Set<Domain.Calendar.CalendarEvent>().Add(evt);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new ProposeAlternativeTimesQuery(familyId.Value, evt.Id.Value, 3, Guid.NewGuid()),
            CancellationToken.None);

        result.Suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_TimedEvent_ReturnsSuggestions()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var evt = CalendarTestHelpers.MakeEvent(
            familyId, "Meeting",
            new DateOnly(2026, 4, 1), new TimeOnly(10, 0),
            new DateOnly(2026, 4, 1), new TimeOnly(11, 0));
        db.Set<Domain.Calendar.CalendarEvent>().Add(evt);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new ProposeAlternativeTimesQuery(familyId.Value, evt.Id.Value, 3, Guid.NewGuid()),
            CancellationToken.None);

        result.Suggestions.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_SuggestionCountRespected()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var evt = CalendarTestHelpers.MakeEvent(
            familyId, "Short Meeting",
            new DateOnly(2026, 4, 1), new TimeOnly(9, 0),
            new DateOnly(2026, 4, 1), new TimeOnly(9, 30));
        db.Set<Domain.Calendar.CalendarEvent>().Add(evt);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new ProposeAlternativeTimesQuery(familyId.Value, evt.Id.Value, 2, Guid.NewGuid()),
            CancellationToken.None);

        result.Suggestions.Count.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task Handle_SuggestionsAreAfterEventDate()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var eventDate = new DateOnly(2026, 4, 1);
        var evt = CalendarTestHelpers.MakeEvent(
            familyId, "Meeting",
            eventDate, new TimeOnly(10, 0),
            eventDate, new TimeOnly(11, 0));
        db.Set<Domain.Calendar.CalendarEvent>().Add(evt);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new ProposeAlternativeTimesQuery(familyId.Value, evt.Id.Value, 3, Guid.NewGuid()),
            CancellationToken.None);

        // All suggestions should be on days strictly after the event date
        result.Suggestions.Should().OnlyContain(
            s => DateOnly.ParseExact(s.Date, "yyyy-MM-dd") > eventDate);
    }
}
