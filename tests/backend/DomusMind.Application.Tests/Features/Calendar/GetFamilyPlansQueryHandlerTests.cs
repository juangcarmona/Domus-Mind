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
    public async Task Handle_ReturnsAllFamilyPlansOrderedByDate()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        db.Set<Domain.Calendar.CalendarEvent>().AddRange(
            CalendarTestHelpers.MakeEvent(familyId, "Later", new DateOnly(2026, 4, 10)),
            CalendarTestHelpers.MakeEvent(familyId, "Sooner", new DateOnly(2026, 4, 1)));
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

        var myEvent = CalendarTestHelpers.MakeEvent(familyId, "My Event", new DateOnly(2026, 4, 1));
        myEvent.AddParticipant(member);
        var otherEvent = CalendarTestHelpers.MakeEvent(familyId, "Other Event", new DateOnly(2026, 4, 1));

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
        db.Set<Domain.Calendar.CalendarEvent>().AddRange(
            CalendarTestHelpers.MakeEvent(familyId, "In Range", new DateOnly(2026, 4, 3)),
            CalendarTestHelpers.MakeEvent(familyId, "Out of Range", new DateOnly(2026, 4, 20)));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetFamilyPlansQuery(familyId.Value, null, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 5), Guid.NewGuid()),
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
            CalendarTestHelpers.MakeEvent(familyA, "Family A Plan", new DateOnly(2026, 4, 1)),
            CalendarTestHelpers.MakeEvent(familyB, "Family B Plan", new DateOnly(2026, 4, 1)));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetFamilyPlansQuery(familyA.Value, null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.Plans.Should().ContainSingle()
            .Which.Title.Should().Be("Family A Plan");
    }
}
