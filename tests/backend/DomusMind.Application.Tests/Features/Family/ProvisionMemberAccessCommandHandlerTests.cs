using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Auth;
using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.ProvisionMemberAccess;
using DomusMind.Application.Tests.Features.Auth;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Family;

public sealed class ProvisionMemberAccessCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ProvisionMemberAccessCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubFamilyAuthorizationService? auth = null,
        InMemoryAuthUserRepository? userRepo = null,
        StubFamilyAccessGranter? accessGranter = null,
        StubTemporaryPasswordGenerator? passwordGenerator = null)
        => new(
            db,
            new EventLogWriter(db),
            auth ?? new StubFamilyAuthorizationService(),
            userRepo ?? new InMemoryAuthUserRepository(),
            new StubPasswordHasher(),
            passwordGenerator ?? new StubTemporaryPasswordGenerator(),
            accessGranter ?? new StubFamilyAccessGranter());

    private static async Task<(DomusMindDbContext Db, Domain.Family.Family Family, Guid ManagerUserId, Guid TargetMemberId)>
        BuildFamilyAsync()
    {
        var db = CreateDb();
        var managerUserId = Guid.NewGuid();
        var targetMemberId = MemberId.New();

        var family = Domain.Family.Family.Create(
            FamilyId.New(),
            FamilyName.Create("Test Family"),
            null,
            DateTime.UtcNow);

        family.AddMember(
            MemberId.New(),
            MemberName.Create("Manager"),
            MemberRole.Adult,
            isManager: true,
            birthDate: null,
            DateTime.UtcNow,
            authUserId: managerUserId);

        family.AddMember(
            targetMemberId,
            MemberName.Create("Alice"),
            MemberRole.Adult,
            isManager: false,
            birthDate: null,
            DateTime.UtcNow,
            authUserId: null);

        db.Set<Domain.Family.Family>().Add(family);
        await db.SaveChangesAsync();
        family.ClearDomainEvents();
        return (db, family, managerUserId, targetMemberId.Value);
    }

    [Fact]
    public async Task Handle_ManagerProvisionsMember_ReturnsTemporaryPassword()
    {
        var (db, family, managerUserId, targetMemberId) = await BuildFamilyAsync();
        var userRepo = new InMemoryAuthUserRepository();
        var passwordGen = new StubTemporaryPasswordGenerator("Temp1234");
        var handler = BuildHandler(db, userRepo: userRepo, passwordGenerator: passwordGen);

        var result = await handler.Handle(new ProvisionMemberAccessCommand(
            family.Id.Value, targetMemberId, "alice@home.local", null, managerUserId),
            CancellationToken.None);

        result.TemporaryPassword.Should().Be("Temp1234");
        result.MustChangePassword.Should().BeTrue();
        result.Email.Should().Be("alice@home.local");
    }

    [Fact]
    public async Task Handle_CreatesAuthUserWithMustChangePassword()
    {
        var (db, family, managerUserId, targetMemberId) = await BuildFamilyAsync();
        var userRepo = new InMemoryAuthUserRepository();
        var handler = BuildHandler(db, userRepo: userRepo);

        await handler.Handle(new ProvisionMemberAccessCommand(
            family.Id.Value, targetMemberId, "alice@home.local", "Alice Display", managerUserId),
            CancellationToken.None);

        var user = userRepo.Users.Single(u => u.Email == "alice@home.local");
        user.MustChangePassword.Should().BeTrue();
        user.IsDisabled.Should().BeFalse();
        user.DisplayName.Should().Be("Alice Display");
        user.MemberId.Should().Be(targetMemberId);
    }

    [Fact]
    public async Task Handle_PasswordIsHashed_PlaintextNotStored()
    {
        var (db, family, managerUserId, targetMemberId) = await BuildFamilyAsync();
        var userRepo = new InMemoryAuthUserRepository();
        var passwordGen = new StubTemporaryPasswordGenerator("PlainPass");
        var handler = BuildHandler(db, userRepo: userRepo, passwordGenerator: passwordGen);

        await handler.Handle(new ProvisionMemberAccessCommand(
            family.Id.Value, targetMemberId, "alice@home.local", null, managerUserId),
            CancellationToken.None);

        var user = userRepo.Users.Single(u => u.Email == "alice@home.local");
        // The StubPasswordHasher wraps with "HASHED:" - plaintext must never be stored
        user.PasswordHash.Should().Be("HASHED:PlainPass");
        user.PasswordHash.Should().NotBe("PlainPass");
    }

    [Fact]
    public async Task Handle_LinksMemberToAuthUser()
    {
        var (db, family, managerUserId, targetMemberId) = await BuildFamilyAsync();
        var userRepo = new InMemoryAuthUserRepository();
        var handler = BuildHandler(db, userRepo: userRepo);

        var result = await handler.Handle(new ProvisionMemberAccessCommand(
            family.Id.Value, targetMemberId, "alice@home.local", null, managerUserId),
            CancellationToken.None);

        var savedMember = (await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id))
            .Members.Single(m => m.Id == MemberId.From(targetMemberId));

        savedMember.AuthUserId.Should().Be(result.UserId);
    }

    [Fact]
    public async Task Handle_NonManager_ThrowsAccessDenied()
    {
        var (db, family, _, targetMemberId) = await BuildFamilyAsync();
        var nonManagerUserId = Guid.NewGuid();

        // Add the non-manager to the family auth side only
        var userRepo = new InMemoryAuthUserRepository();
        var secondMemberId = MemberId.New();
        family.AddMember(secondMemberId, MemberName.Create("NonManager"), MemberRole.Adult,
            isManager: false, birthDate: null, DateTime.UtcNow, authUserId: nonManagerUserId);
        await db.SaveChangesAsync();
        family.ClearDomainEvents();

        var handler = BuildHandler(db, userRepo: userRepo);

        var act = () => handler.Handle(new ProvisionMemberAccessCommand(
            family.Id.Value, targetMemberId, "alice@home.local", null, nonManagerUserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_MemberAlreadyLinked_ThrowsInvalidInput()
    {
        var (db, family, managerUserId, _) = await BuildFamilyAsync();

        // Pick the manager member (already linked) as the target
        var managerMember = family.Members.Single(m => m.AuthUserId == managerUserId);
        var handler = BuildHandler(db);

        var act = () => handler.Handle(new ProvisionMemberAccessCommand(
            family.Id.Value, managerMember.Id.Value, "manager@home.local", null, managerUserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsInvalidInput()
    {
        var (db, family, managerUserId, targetMemberId) = await BuildFamilyAsync();
        var userRepo = new InMemoryAuthUserRepository(
        [
            new AuthUserRecord(Guid.NewGuid(), "alice@home.local", "hash", false),
        ]);
        var handler = BuildHandler(db, userRepo: userRepo);

        var act = () => handler.Handle(new ProvisionMemberAccessCommand(
            family.Id.Value, targetMemberId, "alice@home.local", null, managerUserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_GrantsFamilyAccess()
    {
        var (db, family, managerUserId, targetMemberId) = await BuildFamilyAsync();
        var accessGranter = new StubFamilyAccessGranter();
        var handler = BuildHandler(db, accessGranter: accessGranter);

        var result = await handler.Handle(new ProvisionMemberAccessCommand(
            family.Id.Value, targetMemberId, "alice@home.local", null, managerUserId),
            CancellationToken.None);

        accessGranter.GrantedAccesses.Should().Contain((result.UserId, family.Id.Value));
    }
}

/// <summary>Deterministic password generator for tests.</summary>
internal sealed class StubTemporaryPasswordGenerator : ITemporaryPasswordGenerator
{
    private readonly string _password;

    public StubTemporaryPasswordGenerator(string password = "TestPass1")
        => _password = password;

    public string Generate() => _password;
}
