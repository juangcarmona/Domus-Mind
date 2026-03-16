using DomusMind.Application.Features.Responsibilities;
using DomusMind.Application.Features.Responsibilities.GetHouseholdAreas;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.Responsibilities.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Responsibilities;

public sealed class GetHouseholdAreasQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetHouseholdAreasQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubResponsibilitiesAuthorizationService? auth = null)
        => new(db, auth ?? new StubResponsibilitiesAuthorizationService());

    private static Domain.Family.Family MakeFamily(FamilyId familyId, MemberId memberId, string name = "Alice")
    {
        var family = Domain.Family.Family.Create(familyId, FamilyName.Create("Test"), null, DateTime.UtcNow);
        family.AddMember(memberId, MemberName.Create(name), MemberRole.Create("Adult"), DateTime.UtcNow);
        family.ClearDomainEvents();
        return family;
    }

    private static ResponsibilityDomain MakeDomain(FamilyId familyId, string name, MemberId? owner = null)
    {
        var d = ResponsibilityDomain.Create(
            ResponsibilityDomainId.New(), familyId,
            ResponsibilityAreaName.Create(name), DateTime.UtcNow);
        if (owner.HasValue) d.AssignPrimaryOwner(owner.Value);
        d.ClearDomainEvents();
        return d;
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsResponsibilitiesException()
    {
        var db = CreateDb();
        var auth = new StubResponsibilitiesAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new GetHouseholdAreasQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ResponsibilitiesException>()
            .Where(e => e.Code == ResponsibilitiesErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_ReturnsAllAreasForFamily()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var member = MemberId.New();

        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, member));
        db.Set<ResponsibilityDomain>().AddRange(
            MakeDomain(familyId, "Finance"),
            MakeDomain(familyId, "Maintenance"));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdAreasQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Areas.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_AreaWithPrimaryOwner_IncludesOwnerName()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var member = MemberId.New();

        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, member, "Alice"));
        db.Set<ResponsibilityDomain>().Add(MakeDomain(familyId, "Finance", member));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdAreasQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        var area = result.Areas.Single();
        area.PrimaryOwnerId.Should().Be(member.Value);
        area.PrimaryOwnerName.Should().Be("Alice");
    }

    [Fact]
    public async Task Handle_AreaWithNoOwner_HasNullOwner()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var member = MemberId.New();

        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, member));
        db.Set<ResponsibilityDomain>().Add(MakeDomain(familyId, "Chores"));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdAreasQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        var area = result.Areas.Single();
        area.PrimaryOwnerId.Should().BeNull();
        area.PrimaryOwnerName.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ExcludesOtherFamilyAreas()
    {
        var db = CreateDb();
        var familyA = FamilyId.New();
        var familyB = FamilyId.New();
        var memberA = MemberId.New();
        var memberB = MemberId.New();

        db.Set<Domain.Family.Family>().AddRange(
            MakeFamily(familyA, memberA),
            MakeFamily(familyB, memberB));
        db.Set<ResponsibilityDomain>().AddRange(
            MakeDomain(familyA, "Finance"),
            MakeDomain(familyB, "Maintenance"));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdAreasQuery(familyA.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Areas.Should().ContainSingle()
            .Which.Name.Should().Be("Finance");
    }
}
