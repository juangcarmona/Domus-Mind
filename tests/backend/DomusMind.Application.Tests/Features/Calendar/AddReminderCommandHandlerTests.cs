using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Features.Calendar.AddReminder;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Calendar;

public sealed class AddReminderCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static AddReminderCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubCalendarAuthorizationService? auth = null)
        => new(db, new EventLogWriter(db), auth ?? new StubCalendarAuthorizationService());

    private static async Task<(DomusMindDbContext Db, Domain.Calendar.CalendarEvent Evt)> BuildWithEventAsync()
    {
        var db = CreateDb();
        var calendarEvent = Domain.Calendar.CalendarEvent.Create(
            CalendarEventId.New(),
            FamilyId.New(),
            EventTitle.Create("School Play"),
            null,
            DateTime.UtcNow.AddDays(7),
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
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new AddReminderCommand(evt.Id.Value, 30, Guid.NewGuid()),
            CancellationToken.None);

        result.CalendarEventId.Should().Be(evt.Id.Value);
        result.MinutesBefore.Should().Be(30);
    }

    [Fact]
    public async Task Handle_EventNotFound_ThrowsCalendarException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new AddReminderCommand(Guid.NewGuid(), 15, Guid.NewGuid()),
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
            new AddReminderCommand(evt.Id.Value, 60, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_DuplicateOffset_ThrowsCalendarException()
    {
        var (db, evt) = await BuildWithEventAsync();
        var handler = BuildHandler(db);

        await handler.Handle(
            new AddReminderCommand(evt.Id.Value, 30, Guid.NewGuid()),
            CancellationToken.None);

        var act = () => handler.Handle(
            new AddReminderCommand(evt.Id.Value, 30, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.DuplicateReminderOffset);
    }

    [Fact]
    public async Task Handle_InvalidOffset_ThrowsCalendarException()
    {
        var (db, evt) = await BuildWithEventAsync();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new AddReminderCommand(evt.Id.Value, 0, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.InvalidInput);
    }
}
