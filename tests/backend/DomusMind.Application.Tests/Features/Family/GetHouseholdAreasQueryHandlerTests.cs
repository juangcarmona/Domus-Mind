using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.GetHouseholdAreas;
using DomusMind.Domain.Areas;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Family;

public sealed class GetHouseholdAreasQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetHouseholdAreasQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubFamilyTimelineAuthorizationService? auth = null)
        => new(db, auth ?? new StubFamilyTimelineAuthorizationService());

    private static Domain.Family.Family MakeFamily(FamilyId familyId, MemberId memberId, string name = "Alice")
    {
        var family = Domain.Family.Family.Create(familyId, FamilyName.Create("Test"), null, DateTime.UtcNow);
        family.AddMember(memberId, MemberName.Create(name), MemberRole.Create("Adult"), DateTime.UtcNow);
        family.ClearDomainEvents();
        return family;
    }

    private static Area MakeArea(FamilyId familyId, string name, string? color = null)
        => new(Guid.NewGuid(), familyId, name, color, DateTime.UtcNow);

    [Fact]
    public async Task Handle_AccessDenied_ThrowsFamilyException()
    {
        var db = CreateDb();
        var auth = new StubFamilyTimelineAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new GetHouseholdAreasQuery(Guid.NewGuid(), Guid.NewGuid()),
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
            new GetHouseholdAreasQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.FamilyNotFound);
    }

    [Fact]
    public async Task Handle_ReturnsAllAreasForFamily()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var member = MemberId.New();

        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, member));
        db.Set<Area>().AddRange(
            MakeArea(familyId, "House", "#3B82F6"),
            MakeArea(familyId, "Pets", "#22C55E"));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdAreasQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Areas.Should().HaveCount(2);
        result.Areas.Select(a => a.Name).Should().Equal("House", "Pets");
    }

    [Fact]
    public async Task Handle_IncludesConfiguredColor()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var member = MemberId.New();

        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, member));
        db.Set<Area>().Add(MakeArea(familyId, "Leisure", "#A855F7"));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdAreasQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Areas.Single().Color.Should().Be("#A855F7");
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
        db.Set<Area>().AddRange(
            MakeArea(familyA, "House"),
            MakeArea(familyB, "Admin"));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetHouseholdAreasQuery(familyA.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Areas.Should().ContainSingle()
            .Which.Name.Should().Be("House");
    }
}
