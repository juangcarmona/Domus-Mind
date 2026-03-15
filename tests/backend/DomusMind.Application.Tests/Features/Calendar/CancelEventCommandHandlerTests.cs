using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Features.Calendar.CancelEvent;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Calendar;

public sealed class CancelEventCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static CancelEventCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubCalendarAuthorizationService? auth = null)
        => new(db, new EventLogWriter(db), auth ?? new StubCalendarAuthorizationService());

    private static async Task<(DomusMindDbContext Db, Domain.Calendar.CalendarEvent Evt)> BuildWithEventAsync()
    {
        var db = CreateDb();
        var calendarEvent = Domain.Calendar.CalendarEvent.Create(
            CalendarEventId.New(),
            FamilyId.New(),
            EventTitle.Create("Dentist Appointment"),
            null,
            DateTime.UtcNow.AddDays(2),
            null,
            DateTime.UtcNow);
        db.Set<Domain.Calendar.CalendarEvent>().Add(calendarEvent);
        await db.SaveChangesAsync();
        calendarEvent.ClearDomainEvents();
        return (db, calendarEvent);
    }

    [Fact]
    public async Task Handle_WithValidInput_ReturnsCancelledStatus()
    {
        var (db, evt) = await BuildWithEventAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new CancelEventCommand(evt.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.CalendarEventId.Should().Be(evt.Id.Value);
        result.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task Handle_PersistsCancelledStatusToDatabase()
    {
        var (db, evt) = await BuildWithEventAsync();
        var handler = BuildHandler(db);

        await handler.Handle(
            new CancelEventCommand(evt.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<Domain.Calendar.CalendarEvent>()
            .SingleOrDefaultAsync(e => e.Id == evt.Id);
        saved!.Status.Should().Be(EventStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_EventNotFound_ThrowsCalendarException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new CancelEventCommand(Guid.NewGuid(), Guid.NewGuid()),
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
            new CancelEventCommand(evt.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_AlreadyCancelled_ThrowsCalendarException()
    {
        var (db, evt) = await BuildWithEventAsync();
        evt.Cancel();
        await db.SaveChangesAsync();
        evt.ClearDomainEvents();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new CancelEventCommand(evt.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.EventAlreadyCancelled);
    }
}
