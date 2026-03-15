using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Features.Calendar.RescheduleEvent;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Calendar;

public sealed class RescheduleEventCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static RescheduleEventCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubCalendarAuthorizationService? auth = null)
        => new(db, new EventLogWriter(db), auth ?? new StubCalendarAuthorizationService());

    private static async Task<(DomusMindDbContext Db, Domain.Calendar.CalendarEvent Evt)> BuildWithEventAsync()
    {
        var db = CreateDb();
        var calendarEvent = Domain.Calendar.CalendarEvent.Create(
            CalendarEventId.New(),
            FamilyId.New(),
            EventTitle.Create("Soccer Training"),
            null,
            DateTime.UtcNow.AddDays(1),
            null,
            DateTime.UtcNow);
        db.Set<Domain.Calendar.CalendarEvent>().Add(calendarEvent);
        await db.SaveChangesAsync();
        calendarEvent.ClearDomainEvents();
        return (db, calendarEvent);
    }

    [Fact]
    public async Task Handle_WithValidInput_ReturnsResponse()
    {
        var (db, evt) = await BuildWithEventAsync();
        var newStart = DateTime.UtcNow.AddDays(3);
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new RescheduleEventCommand(evt.Id.Value, newStart, null, Guid.NewGuid()),
            CancellationToken.None);

        result.CalendarEventId.Should().Be(evt.Id.Value);
        result.NewStartTime.Should().Be(newStart);
        result.Status.Should().Be("Scheduled");
    }

    [Fact]
    public async Task Handle_PersistsNewStartTime()
    {
        var (db, evt) = await BuildWithEventAsync();
        var newStart = DateTime.UtcNow.AddDays(5);
        var handler = BuildHandler(db);

        await handler.Handle(
            new RescheduleEventCommand(evt.Id.Value, newStart, null, Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<Domain.Calendar.CalendarEvent>()
            .SingleOrDefaultAsync(e => e.Id == evt.Id);
        saved!.StartTime.Should().Be(newStart);
    }

    [Fact]
    public async Task Handle_EventNotFound_ThrowsCalendarException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new RescheduleEventCommand(Guid.NewGuid(), DateTime.UtcNow.AddDays(1), null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.EventNotFound);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsCalendarException()
    {
        var (db, evt) = await BuildWithEventAsync();
        var auth = new StubCalendarAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new RescheduleEventCommand(evt.Id.Value, DateTime.UtcNow.AddDays(2), null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_CancelledEvent_ThrowsCalendarException()
    {
        var (db, evt) = await BuildWithEventAsync();
        evt.Cancel();
        await db.SaveChangesAsync();
        evt.ClearDomainEvents();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new RescheduleEventCommand(evt.Id.Value, DateTime.UtcNow.AddDays(2), null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.EventAlreadyCancelled);
    }
}
