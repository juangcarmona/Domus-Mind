using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.GetMemberDetails;
using DomusMind.Application.Tests.Features.Auth;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Family;

public sealed class GetMemberDetailsQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetMemberDetailsQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubFamilyAuthorizationService? auth = null,
        InMemoryAuthUserRepository? users = null)
        => new(db, auth ?? new StubFamilyAuthorizationService(), users ?? new InMemoryAuthUserRepository());

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
    public async Task Handle_ReturnsDetailForExistingMember()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var memberId = MemberId.New();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(memberId, MemberName.Create("Alice"), MemberRole.Adult, DateTime.UtcNow);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var result = await handler.Handle(
            new GetMemberDetailsQuery(family.Id.Value, memberId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.MemberId.Should().Be(memberId.Value);
        result.Name.Should().Be("Alice");
        result.FamilyId.Should().Be(family.Id.Value);
        result.AvatarInitial.Should().Be("A");
    }

    [Fact]
    public async Task Handle_MemberNotFound_ThrowsFamilyException()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new GetMemberDetailsQuery(family.Id.Value, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.MemberNotFound);
    }

    [Fact]
    public async Task Handle_FamilyNotFound_ThrowsFamilyException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new GetMemberDetailsQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.FamilyNotFound);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsFamilyException()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var auth = new StubFamilyAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new GetMemberDetailsQuery(family.Id.Value, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_IsCurrentUser_TrueWhenRequesterMatchesMember()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var memberId = MemberId.New();
        var authUserId = Guid.NewGuid();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(memberId, MemberName.Create("Alice"), MemberRole.Adult,
            isManager: false, birthDate: null, DateTime.UtcNow, authUserId: authUserId);
        await db.SaveChangesAsync();

        var users = new InMemoryAuthUserRepository(
        [
            new AuthUserRecord(authUserId, "alice@home.local", "HASHED:pw"),
        ]);

        var handler = BuildHandler(db, users: users);
        var result = await handler.Handle(
            new GetMemberDetailsQuery(family.Id.Value, memberId.Value, authUserId),
            CancellationToken.None);

        result.IsCurrentUser.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReturnsProfileFields_WhenSet()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var memberId = MemberId.New();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        var m = loaded.AddMember(memberId, MemberName.Create("Alice"), MemberRole.Adult, DateTime.UtcNow);
        loaded.UpdateMemberProfile(memberId, "Ali", "+1-555-0100", "ali@home.local", "Night owl", DateTime.UtcNow);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var result = await handler.Handle(
            new GetMemberDetailsQuery(family.Id.Value, memberId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.PreferredName.Should().Be("Ali");
        result.PrimaryPhone.Should().Be("+1-555-0100");
        result.PrimaryEmail.Should().Be("ali@home.local");
        result.HouseholdNote.Should().Be("Night owl");
        // Avatar uses preferred name
        result.AvatarInitial.Should().Be("A");
    }
}
