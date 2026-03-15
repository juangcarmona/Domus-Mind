using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Features.Calendar.GetFamilyPlans;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Calendar;

public sealed class GetFamilyPlansQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetFamilyPlansQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubCalendarAuthorizationService? auth = null)
        => new(db, auth ?? new StubCalendarAuthorizationService());

    private static Domain.Calendar.CalendarEvent MakeEvent(
        FamilyId familyId, string title, DateTime? start = null)
        => Domain.Calendar.CalendarEvent.Create(
            CalendarEventId.New(), familyId,
            EventTitle.Create(title), null,
            start ?? DateTime.UtcNow.AddDays(1), null, DateTime.UtcNow);

    [Fact]
    public async Task Handle_AccessDenied_ThrowsCalendarException()
    {
        var db = CreateDb();
        var auth = new StubCalendarAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new GetFamilyPlansQuery(Guid.NewGuid(), null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_ReturnsAllFamilyPlansOrderedByStartTime()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var now = DateTime.UtcNow;
        db.Set<Domain.Calendar.CalendarEvent>().AddRange(
            MakeEvent(familyId, "Later", now.AddDays(5)),
            MakeEvent(familyId, "Sooner", now.AddDays(1)));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetFamilyPlansQuery(familyId.Value, null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.Plans.Should().HaveCount(2);
        result.Plans.First().Title.Should().Be("Sooner");
    }

    [Fact]
    public async Task Handle_MemberFilter_ReturnsOnlyEventsWithMemberAsParticipant()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var member = MemberId.New();

        var myEvent = MakeEvent(familyId, "My Event");
        myEvent.AddParticipant(member);
        var otherEvent = MakeEvent(familyId, "Other Event"); // no participants

        db.Set<Domain.Calendar.CalendarEvent>().AddRange(myEvent, otherEvent);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetFamilyPlansQuery(familyId.Value, member.Value, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.Plans.Should().ContainSingle()
            .Which.Title.Should().Be("My Event");
    }

    [Fact]
    public async Task Handle_DateFilter_ReturnsOnlyEventsInRange()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var now = DateTime.UtcNow;
        db.Set<Domain.Calendar.CalendarEvent>().AddRange(
            MakeEvent(familyId, "In Range", now.AddDays(3)),
            MakeEvent(familyId, "Out of Range", now.AddDays(10)));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetFamilyPlansQuery(familyId.Value, null, now, now.AddDays(5), Guid.NewGuid()),
            CancellationToken.None);

        result.Plans.Should().ContainSingle()
            .Which.Title.Should().Be("In Range");
    }

    [Fact]
    public async Task Handle_ExcludesOtherFamilyPlans()
    {
        var db = CreateDb();
        var familyA = FamilyId.New();
        var familyB = FamilyId.New();
        db.Set<Domain.Calendar.CalendarEvent>().AddRange(
            MakeEvent(familyA, "Family A Plan"),
            MakeEvent(familyB, "Family B Plan"));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetFamilyPlansQuery(familyA.Value, null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.Plans.Should().ContainSingle()
            .Which.Title.Should().Be("Family A Plan");
    }
}
