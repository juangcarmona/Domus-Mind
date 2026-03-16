using DomusMind.Application.Features.Responsibilities;
using DomusMind.Application.Features.Responsibilities.GetResponsibilityVisibility;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.Responsibilities.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Responsibilities;

public sealed class GetResponsibilityVisibilityQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetResponsibilityVisibilityQueryHandler BuildHandler(
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
            new GetResponsibilityVisibilityQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ResponsibilitiesException>()
            .Where(e => e.Code == ResponsibilitiesErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_PrimaryOwner_ShowsCorrectRole()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var member = MemberId.New();

        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, (member, "Alice")));

        var domain = MakeDomain(familyId, "Finance");
        domain.AssignPrimaryOwner(member);
        db.Set<ResponsibilityDomain>().Add(domain);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetResponsibilityVisibilityQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        var view = result.Views.Single();
        view.Connections.Should().ContainSingle()
            .Which.Role.Should().Be("PrimaryOwner");
    }

    [Fact]
    public async Task Handle_SecondaryOwner_ShowsCorrectRole()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var primary = MemberId.New();
        var secondary = MemberId.New();

        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, (primary, "Alice"), (secondary, "Bob")));

        var domain = MakeDomain(familyId, "Chores");
        domain.AssignPrimaryOwner(primary);
        domain.AssignSecondaryOwner(secondary);
        db.Set<ResponsibilityDomain>().Add(domain);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetResponsibilityVisibilityQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        var bobView = result.Views.Single(v => v.MemberId == secondary.Value);
        bobView.Connections.Should().ContainSingle()
            .Which.Role.Should().Be("SecondaryOwner");
    }

    [Fact]
    public async Task Handle_MemberWithNoConnections_HasEmptyConnections()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var member = MemberId.New();

        db.Set<Domain.Family.Family>().Add(MakeFamily(familyId, (member, "Alice")));
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetResponsibilityVisibilityQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Views.Single().Connections.Should().BeEmpty();
    }
}
