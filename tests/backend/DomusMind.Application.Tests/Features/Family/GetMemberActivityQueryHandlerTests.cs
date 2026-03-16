using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.GetMemberActivity;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.Responsibilities.ValueObjects;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Family;

public sealed class GetMemberActivityQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetMemberActivityQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubFamilyTimelineAuthorizationService? auth = null)
        => new(db, auth ?? new StubFamilyTimelineAuthorizationService());

    private static Domain.Family.Family MakeFamily(FamilyId familyId, MemberId memberId, string memberName = "Alice")
    {
        var family = Domain.Family.Family.Create(familyId, FamilyName.Create("Test Family"), null, DateTime.UtcNow);
        family.AddMember(memberId, MemberName.Create(memberName), MemberRole.Create("Adult"), DateTime.UtcNow);
        family.ClearDomainEvents();
        return family;
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsFamilyException()
    {
        var db = CreateDb();
        var auth = new StubFamilyTimelineAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new GetMemberActivityQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_FamilyNotFound_ThrowsFamilyException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new GetMemberActivityQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.FamilyNotFound);
    }

    [Fact]
    public async Task Handle_IncludesCalendarEventsWhereParticipant()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var memberId = MemberId.New();

        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, memberId));

        var evt = Domain.Calendar.CalendarEvent.Create(
            CalendarEventId.New(), familyId,
            EventTitle.Create("School Trip"), null,
            DateTime.UtcNow.AddDays(3), null, DateTime.UtcNow);
        evt.AddParticipant(memberId);
        db.Set<Domain.Calendar.CalendarEvent>().Add(evt);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetMemberActivityQuery(familyId.Value, memberId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.CalendarEvents.Should().ContainSingle()
            .Which.Title.Should().Be("School Trip");
    }

    [Fact]
    public async Task Handle_IncludesTasksAssignedToMember()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var memberId = MemberId.New();

        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, memberId));

        var task = HouseholdTask.Create(
            TaskId.New(), familyId,
            TaskTitle.Create("Fix Fence"), null,
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow);
        task.Assign(memberId);
        task.ClearDomainEvents();
        db.Set<HouseholdTask>().Add(task);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetMemberActivityQuery(familyId.Value, memberId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Tasks.Should().ContainSingle()
            .Which.Title.Should().Be("Fix Fence");
    }

    [Fact]
    public async Task Handle_IncludesResponsibilityDomainAsPrimaryOwner()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var memberId = MemberId.New();

        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, memberId));

        var domain = ResponsibilityDomain.Create(
            ResponsibilityDomainId.New(), familyId,
            ResponsibilityAreaName.Create("Finance"), DateTime.UtcNow);
        domain.AssignPrimaryOwner(memberId);
        domain.ClearDomainEvents();
        db.Set<ResponsibilityDomain>().Add(domain);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetMemberActivityQuery(familyId.Value, memberId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Responsibilities.Should().ContainSingle()
            .Which.Role.Should().Be("PrimaryOwner");
    }

    [Fact]
    public async Task Handle_EmptyActivity_ReturnsAllEmptyCollections()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var memberId = MemberId.New();

        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, memberId));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetMemberActivityQuery(familyId.Value, memberId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.CalendarEvents.Should().BeEmpty();
        result.Tasks.Should().BeEmpty();
        result.Responsibilities.Should().BeEmpty();
    }
}
