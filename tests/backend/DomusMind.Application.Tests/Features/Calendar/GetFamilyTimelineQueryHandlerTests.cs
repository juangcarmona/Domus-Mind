using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Features.Calendar.GetFamilyTimeline;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Calendar;

public sealed class GetFamilyTimelineQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetFamilyTimelineQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubCalendarAuthorizationService? auth = null)
        => new(db, auth ?? new StubCalendarAuthorizationService());

    private static Domain.Calendar.CalendarEvent MakeEvent(
        FamilyId familyId,
        string title,
        DateTime startTime,
        DateTime? endTime = null)
        => Domain.Calendar.CalendarEvent.Create(
            CalendarEventId.New(),
            familyId,
            EventTitle.Create(title),
            null,
            startTime,
            endTime,
            DateTime.UtcNow);

    [Fact]
    public async Task Handle_WithFamilyEvents_ReturnsOrderedByStartTime()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var later = MakeEvent(familyId, "Later Event", DateTime.UtcNow.AddDays(5));
        var sooner = MakeEvent(familyId, "Sooner Event", DateTime.UtcNow.AddDays(1));
        db.Set<Domain.Calendar.CalendarEvent>().AddRange(later, sooner);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetFamilyTimelineQuery(familyId.Value, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.Events.Should().HaveCount(2);
        result.Events.First().Title.Should().Be("Sooner Event");
        result.Events.Last().Title.Should().Be("Later Event");
    }

    [Fact]
    public async Task Handle_NoEvents_ReturnsEmptyList()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetFamilyTimelineQuery(Guid.NewGuid(), null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.Events.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsCalendarException()
    {
        var db = CreateDb();
        var auth = new StubCalendarAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new GetFamilyTimelineQuery(Guid.NewGuid(), null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_FiltersByFromDate()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var past = MakeEvent(familyId, "Past Event", DateTime.UtcNow.AddDays(-3));
        var future = MakeEvent(familyId, "Future Event", DateTime.UtcNow.AddDays(3));
        db.Set<Domain.Calendar.CalendarEvent>().AddRange(past, future);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetFamilyTimelineQuery(familyId.Value, DateTime.UtcNow, null, Guid.NewGuid()),
            CancellationToken.None);

        result.Events.Should().ContainSingle()
            .Which.Title.Should().Be("Future Event");
    }

    [Fact]
    public async Task Handle_ExcludesOtherFamilyEvents()
    {
        var db = CreateDb();
        var familyA = FamilyId.New();
        var familyB = FamilyId.New();
        var eventA = MakeEvent(familyA, "Family A Event", DateTime.UtcNow.AddDays(1));
        var eventB = MakeEvent(familyB, "Family B Event", DateTime.UtcNow.AddDays(1));
        db.Set<Domain.Calendar.CalendarEvent>().AddRange(eventA, eventB);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetFamilyTimelineQuery(familyA.Value, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.Events.Should().ContainSingle()
            .Which.Title.Should().Be("Family A Event");
    }
}
