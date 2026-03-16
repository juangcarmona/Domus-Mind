using DomusMind.Application.Features.Responsibilities;
using DomusMind.Application.Features.Responsibilities.SuggestResponsibilityOwner;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.Responsibilities.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Responsibilities;

public sealed class SuggestResponsibilityOwnerQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static SuggestResponsibilityOwnerQueryHandler BuildHandler(
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

    private static ResponsibilityDomain MakeDomain(FamilyId familyId, string name, MemberId? owner = null)
    {
        var domain = ResponsibilityDomain.Create(
            ResponsibilityDomainId.New(), familyId,
            ResponsibilityAreaName.Create(name), DateTime.UtcNow);
        if (owner.HasValue) domain.AssignPrimaryOwner(owner.Value);
        domain.ClearDomainEvents();
        return domain;
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsResponsibilitiesException()
    {
        var db = CreateDb();
        var auth = new StubResponsibilitiesAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new SuggestResponsibilityOwnerQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ResponsibilitiesException>()
            .Where(e => e.Code == ResponsibilitiesErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_DomainNotFound_ThrowsResponsibilitiesException()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var family = MakeFamily(familyId, (MemberId.New(), "Alice"));
        db.Set<Domain.Family.Family>().Add(family);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new SuggestResponsibilityOwnerQuery(familyId.Value, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ResponsibilitiesException>()
            .Where(e => e.Code == ResponsibilitiesErrorCode.ResponsibilityDomainNotFound);
    }

    [Fact]
    public async Task Handle_SuggestsLeastLoadedMemberFirst()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var member1 = MemberId.New();
        var member2 = MemberId.New();

        var family = MakeFamily(familyId, (member1, "Alice"), (member2, "Bob"));
        db.Set<Domain.Family.Family>().Add(family);

        // member1 already owns one domain
        var existingDomain = MakeDomain(familyId, "Finance", member1);
        var targetDomain = MakeDomain(familyId, "Maintenance");
        db.Set<ResponsibilityDomain>().AddRange(existingDomain, targetDomain);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new SuggestResponsibilityOwnerQuery(familyId.Value, targetDomain.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Suggestions.Should().HaveCount(2);
        result.Suggestions.First().MemberId.Should().Be(member2.Value); // least loaded
    }
}
