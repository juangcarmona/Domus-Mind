using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.GetFamilyMembers;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Family;

public sealed class GetFamilyMembersQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetFamilyMembersQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubFamilyAuthorizationService? auth = null)
        => new(db, auth ?? new StubFamilyAuthorizationService());

    private static async Task<(DomusMindDbContext Db, Domain.Family.Family Family)> BuildWithFamilyAsync()
    {
        var db = CreateDb();
        var family = Domain.Family.Family.Create(
            FamilyId.New(),
            FamilyName.Create("Test Family"),
            null,
            DateTime.UtcNow);
        db.Set<Domain.Family.Family>().Add(family);
        await db.SaveChangesAsync();
        return (db, family);
    }

    [Fact]
    public async Task Handle_FamilyWithNoMembers_ReturnsEmptyList()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetFamilyMembersQuery(family.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_FamilyWithMembers_ReturnsCorrectItems()
    {
        var (db, family) = await BuildWithFamilyAsync();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(MemberId.New(), MemberName.Create("Alice"), MemberRole.Adult, DateTime.UtcNow);
        loaded.AddMember(MemberId.New(), MemberName.Create("Bob"), MemberRole.Child, DateTime.UtcNow);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var result = await handler.Handle(
            new GetFamilyMembersQuery(family.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(m => m.Name).Should().BeEquivalentTo(["Alice", "Bob"]);
    }

    [Fact]
    public async Task Handle_MemberResponse_ContainsCorrectFamilyId()
    {
        var (db, family) = await BuildWithFamilyAsync();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(MemberId.New(), MemberName.Create("Carol"), MemberRole.Caregiver, DateTime.UtcNow);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var result = await handler.Handle(
            new GetFamilyMembersQuery(family.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Single().FamilyId.Should().Be(family.Id.Value);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsFamilyException()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var auth = new StubFamilyAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new GetFamilyMembersQuery(family.Id.Value, Guid.NewGuid()),
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
            new GetFamilyMembersQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.FamilyNotFound);
    }
}
