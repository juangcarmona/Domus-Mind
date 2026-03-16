using DomusMind.Application.Features.Responsibilities;
using DomusMind.Application.Features.Responsibilities.DetectResponsibilityOverload;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.Responsibilities.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Responsibilities;

public sealed class DetectResponsibilityOverloadQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static DetectResponsibilityOverloadQueryHandler BuildHandler(
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

    private static ResponsibilityDomain MakeOwnedDomain(FamilyId familyId, string name, MemberId owner)
    {
        var d = ResponsibilityDomain.Create(
            ResponsibilityDomainId.New(), familyId,
            ResponsibilityAreaName.Create(name), DateTime.UtcNow);
        d.AssignPrimaryOwner(owner);
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
            new DetectResponsibilityOverloadQuery(Guid.NewGuid(), 3, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ResponsibilitiesException>()
            .Where(e => e.Code == ResponsibilitiesErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_MemberBelowThreshold_NotInOverloaded()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var member = MemberId.New();

        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, member));
        db.Set<ResponsibilityDomain>().AddRange(
            MakeOwnedDomain(familyId, "Finance", member),
            MakeOwnedDomain(familyId, "Food", member));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new DetectResponsibilityOverloadQuery(familyId.Value, 3, Guid.NewGuid()),
            CancellationToken.None);

        result.Overloaded.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MemberAboveThreshold_ReturnedAsOverloaded()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var member = MemberId.New();

        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, member, "Alice"));
        db.Set<ResponsibilityDomain>().AddRange(
            MakeOwnedDomain(familyId, "Finance", member),
            MakeOwnedDomain(familyId, "Food", member),
            MakeOwnedDomain(familyId, "Maintenance", member),
            MakeOwnedDomain(familyId, "School", member)); // 4 domains > threshold 3
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new DetectResponsibilityOverloadQuery(familyId.Value, 3, Guid.NewGuid()),
            CancellationToken.None);

        result.Overloaded.Should().ContainSingle()
            .Which.TotalLoad.Should().Be(4);
    }

    [Fact]
    public async Task Handle_ThresholdInResponse_MatchesQuery()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var member = MemberId.New();
        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, member));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new DetectResponsibilityOverloadQuery(familyId.Value, 5, Guid.NewGuid()),
            CancellationToken.None);

        result.Threshold.Should().Be(5);
    }
}
