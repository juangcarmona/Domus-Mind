using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Features.Calendar.AddEventParticipant;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Calendar;

public sealed class AddEventParticipantCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static AddEventParticipantCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubCalendarAuthorizationService? auth = null)
        => new(db, new EventLogWriter(db), auth ?? new StubCalendarAuthorizationService());

    private static async Task<(DomusMindDbContext Db, Domain.Calendar.CalendarEvent Evt)> BuildWithEventAsync()
    {
        var db = CreateDb();
        var calendarEvent = Domain.Calendar.CalendarEvent.Create(
            CalendarEventId.New(),
            FamilyId.New(),
            EventTitle.Create("Family Dinner"),
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
        var memberId = Guid.NewGuid();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new AddEventParticipantCommand(evt.Id.Value, memberId, Guid.NewGuid()),
            CancellationToken.None);

        result.CalendarEventId.Should().Be(evt.Id.Value);
        result.MemberId.Should().Be(memberId);
    }

    [Fact]
    public async Task Handle_EventNotFound_ThrowsCalendarException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new AddEventParticipantCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
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
            new AddEventParticipantCommand(evt.Id.Value, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_DuplicateParticipant_ThrowsCalendarException()
    {
        var (db, evt) = await BuildWithEventAsync();
        var memberId = Guid.NewGuid();
        var handler = BuildHandler(db);

        await handler.Handle(
            new AddEventParticipantCommand(evt.Id.Value, memberId, Guid.NewGuid()),
            CancellationToken.None);

        var act = () => handler.Handle(
            new AddEventParticipantCommand(evt.Id.Value, memberId, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.DuplicateParticipant);
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
            new AddEventParticipantCommand(evt.Id.Value, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.EventAlreadyCancelled);
    }
}
