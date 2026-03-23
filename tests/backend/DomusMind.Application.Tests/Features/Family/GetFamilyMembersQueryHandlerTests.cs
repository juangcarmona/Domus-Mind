using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.GetFamilyMembers;
using DomusMind.Application.Tests.Features.Auth;
using DomusMind.Contracts.Family;
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
        loaded.AddMember(MemberId.New(), MemberName.Create("Carol"), MemberRole.Adult, DateTime.UtcNow);
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

    [Fact]
    public async Task Handle_MemberWithNoAccount_ReturnsNoAccessStatus()
    {
        var (db, family) = await BuildWithFamilyAsync();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(MemberId.New(), MemberName.Create("Alice"), MemberRole.Adult, DateTime.UtcNow);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var result = await handler.Handle(
            new GetFamilyMembersQuery(family.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Single().AccessStatus.Should().Be(MemberAccessStatus.NoAccess);
        result.Single().HasAccount.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_MemberWithActiveAccount_ReturnsActiveStatus()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var authUserId = Guid.NewGuid();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(MemberId.New(), MemberName.Create("Alice"), MemberRole.Adult,
            isManager: false, birthDate: null, DateTime.UtcNow, authUserId: authUserId);
        await db.SaveChangesAsync();

        var users = new InMemoryAuthUserRepository(
        [
            new AuthUserRecord(authUserId, "alice@home.local", "HASHED:pw", MustChangePassword: false),
        ]);

        var handler = BuildHandler(db, users: users);
        var result = await handler.Handle(
            new GetFamilyMembersQuery(family.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Single().AccessStatus.Should().Be(MemberAccessStatus.Active);
        result.Single().HasAccount.Should().BeTrue();
        result.Single().LinkedEmail.Should().Be("alice@home.local");
    }

    [Fact]
    public async Task Handle_MemberWithDisabledAccount_ReturnsDisabledStatus()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var authUserId = Guid.NewGuid();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(MemberId.New(), MemberName.Create("Dave"), MemberRole.Adult,
            isManager: false, birthDate: null, DateTime.UtcNow, authUserId: authUserId);
        await db.SaveChangesAsync();

        var users = new InMemoryAuthUserRepository(
        [
            new AuthUserRecord(authUserId, "dave@home.local", "HASHED:pw", MustChangePassword: false, IsDisabled: true),
        ]);

        var handler = BuildHandler(db, users: users);
        var result = await handler.Handle(
            new GetFamilyMembersQuery(family.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Single().AccessStatus.Should().Be(MemberAccessStatus.Disabled);
    }

    [Fact]
    public async Task Handle_ProvisionedMemberNeverLoggedIn_ReturnsInvitedOrProvisionedStatus()
    {
        // Arrange: account was provisioned (MustChangePassword=true) but LastLoginAtUtc is null
        var (db, family) = await BuildWithFamilyAsync();
        var authUserId = Guid.NewGuid();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(MemberId.New(), MemberName.Create("Eve"), MemberRole.Adult,
            isManager: false, birthDate: null, DateTime.UtcNow, authUserId: authUserId);
        await db.SaveChangesAsync();

        // Stub returns MustChangePassword=true and LastLoginAtUtc=null
        var users = new InMemoryAuthUserRepository(
        [
            new AuthUserRecord(authUserId, "eve@home.local", "HASHED:pw", MustChangePassword: true),
        ]);

        var handler = BuildHandler(db, users: users);
        var result = await handler.Handle(
            new GetFamilyMembersQuery(family.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Single().AccessStatus.Should().Be(MemberAccessStatus.InvitedOrProvisioned);
    }

    [Fact]
    public async Task Handle_IsCurrentUser_TrueWhenAuthUserMatchesRequester()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var requesterUserId = Guid.NewGuid();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(MemberId.New(), MemberName.Create("Myself"), MemberRole.Adult,
            isManager: false, birthDate: null, DateTime.UtcNow, authUserId: requesterUserId);
        await db.SaveChangesAsync();

        var users = new InMemoryAuthUserRepository(
        [
            new AuthUserRecord(requesterUserId, "me@home.local", "HASHED:pw", MustChangePassword: false),
        ]);

        var handler = BuildHandler(db, users: users);
        var result = await handler.Handle(
            new GetFamilyMembersQuery(family.Id.Value, requesterUserId),
            CancellationToken.None);

        result.Single().IsCurrentUser.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CanGrantAccess_TrueForManagerAndMemberWithoutAccount()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var managerUserId = Guid.NewGuid();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(MemberId.New(), MemberName.Create("Manager"), MemberRole.Adult,
            isManager: true, birthDate: null, DateTime.UtcNow, authUserId: managerUserId);
        loaded.AddMember(MemberId.New(), MemberName.Create("Child"), MemberRole.Child, DateTime.UtcNow);
        await db.SaveChangesAsync();

        var users = new InMemoryAuthUserRepository(
        [
            new AuthUserRecord(managerUserId, "manager@home.local", "HASHED:pw", MustChangePassword: false),
        ]);

        var handler = BuildHandler(db, users: users);
        var result = await handler.Handle(
            new GetFamilyMembersQuery(family.Id.Value, managerUserId),
            CancellationToken.None);

        var child = result.Single(m => m.Name == "Child");
        child.CanGrantAccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CanGrantAccess_FalseForPetEvenWhenManager()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var managerUserId = Guid.NewGuid();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(MemberId.New(), MemberName.Create("Manager"), MemberRole.Adult,
            isManager: true, birthDate: null, DateTime.UtcNow, authUserId: managerUserId);
        loaded.AddMember(MemberId.New(), MemberName.Create("Buddy"), MemberRole.Pet, DateTime.UtcNow);
        await db.SaveChangesAsync();

        var users = new InMemoryAuthUserRepository(
        [
            new AuthUserRecord(managerUserId, "manager@home.local", "HASHED:pw", MustChangePassword: false),
        ]);

        var handler = BuildHandler(db, users: users);
        var result = await handler.Handle(
            new GetFamilyMembersQuery(family.Id.Value, managerUserId),
            CancellationToken.None);

        var pet = result.Single(m => m.Name == "Buddy");
        pet.CanGrantAccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AvatarInitial_IsFirstLetterOfNameUpperCased()
    {
        var (db, family) = await BuildWithFamilyAsync();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(MemberId.New(), MemberName.Create("alice"), MemberRole.Adult, DateTime.UtcNow);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var result = await handler.Handle(
            new GetFamilyMembersQuery(family.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Single().AvatarInitial.Should().Be("A");
    }
}
