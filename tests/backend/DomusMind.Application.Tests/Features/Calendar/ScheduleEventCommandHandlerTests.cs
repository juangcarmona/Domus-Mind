using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Features.Calendar.ScheduleEvent;
using DomusMind.Domain.Calendar;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Calendar;

public sealed class ScheduleEventCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ScheduleEventCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubCalendarAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new ScheduleEventCommandHandler(
            context,
            new EventLogWriter(context),
            auth ?? new StubCalendarAuthorizationService());
    }

    [Fact]
    public async Task Handle_WithValidInput_ReturnsResponse()
    {
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var handler = BuildHandler();

        var result = await handler.Handle(
            new ScheduleEventCommand("School Trip", familyId, startDate.ToString("yyyy-MM-dd"), null, null, null, null, userId),
            CancellationToken.None);

        result.CalendarEventId.Should().NotBeEmpty();
        result.FamilyId.Should().Be(familyId);
        result.Title.Should().Be("School Trip");
        result.Date.Should().Be(startDate.ToString("yyyy-MM-dd"));
        result.Status.Should().Be("Scheduled");
    }

    [Fact]
    public async Task Handle_PersistsEventToDatabase()
    {
        var db = CreateDb();
        var handler = BuildHandler(db: db);
        var familyId = Guid.NewGuid();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var result = await handler.Handle(
            new ScheduleEventCommand("Medical Appointment", familyId, startDate.ToString("yyyy-MM-dd"), null, null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<Domain.Calendar.CalendarEvent>()
            .SingleOrDefaultAsync(e => e.Id == CalendarEventId.From(result.CalendarEventId));
        saved.Should().NotBeNull();
        saved!.Title.Value.Should().Be("Medical Appointment");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyTitle_ThrowsCalendarException(string title)
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new ScheduleEventCommand(title, Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)).ToString("yyyy-MM-dd"), null, null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_EndBeforeStart_ThrowsCalendarException()
    {
        var handler = BuildHandler();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(5));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));

        var act = () => handler.Handle(
            new ScheduleEventCommand("Bad Event", Guid.NewGuid(), startDate.ToString("yyyy-MM-dd"), "18:00", endDate.ToString("yyyy-MM-dd"), "16:00", null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsCalendarException()
    {
        var auth = new StubCalendarAuthorizationService { CanAccess = false };
        var handler = BuildHandler(auth: auth);

        var act = () => handler.Handle(
            new ScheduleEventCommand("Trip", Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)).ToString("yyyy-MM-dd"), null, null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.AccessDenied);
    }
}
