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

    private static Domain.Calendar.CalendarEvent MakeEvent(
        FamilyId familyId, string title, DateTime start, DateTime? end = null)
        => Domain.Calendar.CalendarEvent.Create(
            CalendarEventId.New(), familyId,
            EventTitle.Create(title), null,
            start, end, DateTime.UtcNow);

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
    public async Task Handle_EmptyCalendar_ReturnsSuggestions()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var start = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
        var evt = MakeEvent(familyId, "Meeting", start, start.AddHours(1));
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
        var start = DateTime.UtcNow.Date.AddDays(1).AddHours(9);
        var evt = MakeEvent(familyId, "Short Meeting", start, start.AddMinutes(30));
        db.Set<Domain.Calendar.CalendarEvent>().Add(evt);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new ProposeAlternativeTimesQuery(familyId.Value, evt.Id.Value, 2, Guid.NewGuid()),
            CancellationToken.None);

        result.Suggestions.Count.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task Handle_SuggestionsAreAfterEventStartTime()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var start = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
        var evt = MakeEvent(familyId, "Meeting", start, start.AddHours(1));
        db.Set<Domain.Calendar.CalendarEvent>().Add(evt);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new ProposeAlternativeTimesQuery(familyId.Value, evt.Id.Value, 3, Guid.NewGuid()),
            CancellationToken.None);

        result.Suggestions.Should().OnlyContain(s => s.ProposedStart > start);
    }
}
