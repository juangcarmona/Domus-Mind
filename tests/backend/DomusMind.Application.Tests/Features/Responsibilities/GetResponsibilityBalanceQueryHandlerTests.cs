using DomusMind.Application.Features.Responsibilities;
using DomusMind.Application.Features.Responsibilities.GetResponsibilityBalance;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.Responsibilities.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Responsibilities;

public sealed class GetResponsibilityBalanceQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetResponsibilityBalanceQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubResponsibilitiesAuthorizationService? auth = null)
        => new(db, auth ?? new StubResponsibilitiesAuthorizationService());

    private static Domain.Family.Family MakeFamily(FamilyId familyId, params (MemberId id, string name)[] members)
    {
        var family = Domain.Family.Family.Create(familyId, FamilyName.Create("Test"), null, DateTime.UtcNow);
        foreach (var (id, name) in members)
            family.AddMember(id, MemberName.Create(name), MemberRole.Create("Adult"), DateTime.UtcNow);
        family.ClearDomainEvents();
        return family;
    }

    private static ResponsibilityDomain MakeDomain(FamilyId familyId, string name)
    {
        var d = ResponsibilityDomain.Create(
            ResponsibilityDomainId.New(), familyId,
            ResponsibilityAreaName.Create(name), DateTime.UtcNow);
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
            new GetResponsibilityBalanceQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ResponsibilitiesException>()
            .Where(e => e.Code == ResponsibilitiesErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_CountsPrimaryAndSecondaryOwnershipsPerMember()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var member1 = MemberId.New();
        var member2 = MemberId.New();

        var family = MakeFamily(familyId, (member1, "Alice"), (member2, "Bob"));
        db.Set<Domain.Family.Family>().Add(family);

        var domainA = MakeDomain(familyId, "Finance");
        domainA.AssignPrimaryOwner(member1);

        var domainB = MakeDomain(familyId, "Maintenance");
        domainB.AssignPrimaryOwner(member2);
        domainB.AssignSecondaryOwner(member1);

        db.Set<ResponsibilityDomain>().AddRange(domainA, domainB);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetResponsibilityBalanceQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Loads.Should().HaveCount(2);

        var alice = result.Loads.Single(l => l.MemberId == member1.Value);
        alice.PrimaryOwnerships.Should().Be(1);
        alice.SecondaryOwnerships.Should().Be(1);
        alice.TotalLoad.Should().Be(2);

        var bob = result.Loads.Single(l => l.MemberId == member2.Value);
        bob.PrimaryOwnerships.Should().Be(1);
        bob.SecondaryOwnerships.Should().Be(0);
        bob.TotalLoad.Should().Be(1);
    }

    [Fact]
    public async Task Handle_MembersWithNoOwnerships_IncludedWithZeroCounts()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var member1 = MemberId.New();

        var family = MakeFamily(familyId, (member1, "Alice"));
        db.Set<Domain.Family.Family>().Add(family);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetResponsibilityBalanceQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Loads.Should().ContainSingle()
            .Which.TotalLoad.Should().Be(0);
    }
}
