using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.InviteMember;
using DomusMind.Application.Tests.Features.Auth;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Family;

public sealed class InviteMemberCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static InviteMemberCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubFamilyAuthorizationService? auth = null,
        InMemoryAuthUserRepository? userRepo = null,
        StubFamilyAccessGranter? accessGranter = null)
        => new(
            db,
            new EventLogWriter(db),
            auth ?? new StubFamilyAuthorizationService(),
            userRepo ?? new InMemoryAuthUserRepository(),
            new StubPasswordHasher(),
            accessGranter ?? new StubFamilyAccessGranter());

    private static async Task<(DomusMindDbContext Db, Domain.Family.Family Family, Guid ManagerUserId)> BuildFamilyWithManagerAsync()
    {
        var db = CreateDb();
        var managerUserId = Guid.NewGuid();

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

        db.Set<Domain.Family.Family>().Add(family);
        await db.SaveChangesAsync();
        family.ClearDomainEvents();
        return (db, family, managerUserId);
    }

    [Fact]
    public async Task Handle_WithValidInput_CreatesAuthUserAndFamilyMember()
    {
        var (db, family, managerUserId) = await BuildFamilyWithManagerAsync();
        var userRepo = new InMemoryAuthUserRepository();
        var handler = BuildHandler(db, userRepo: userRepo);

        var result = await handler.Handle(
            new InviteMemberCommand(
                family.Id.Value,
                "Alice",
                "Adult",
                null,
                false,
                "alice@household.local",
                "Temp1234!",
                managerUserId),
            CancellationToken.None);

        result.MemberId.Should().NotBeEmpty();
        result.FamilyId.Should().Be(family.Id.Value);
        result.Name.Should().Be("Alice");
        result.Role.Should().Be("Adult");
        result.Username.Should().Be("alice@household.local");

        // Verify auth user was created with MustChangePassword = true
        var savedUser = userRepo.Users.Single(u => u.Email == "alice@household.local");
        savedUser.MustChangePassword.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_PersistsMemberToDatabase()
    {
        var (db, family, managerUserId) = await BuildFamilyWithManagerAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new InviteMemberCommand(
                family.Id.Value,
                "Bob",
                "Adult",
                null,
                false,
                "bob@household.local",
                "Password123",
                managerUserId),
            CancellationToken.None);

        var saved = await db.Set<FamilyMember>().FindAsync(MemberId.From(result.MemberId));
        saved.Should().NotBeNull();
        saved!.AuthUserId.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_GrantsFamilyAccessToNewUser()
    {
        var (db, family, managerUserId) = await BuildFamilyWithManagerAsync();
        var accessGranter = new StubFamilyAccessGranter();
        var handler = BuildHandler(db, accessGranter: accessGranter);

        await handler.Handle(
            new InviteMemberCommand(
                family.Id.Value,
                "Carol",
                "Adult",
                null,
                false,
                "carol@household.local",
                "Password123",
                managerUserId),
            CancellationToken.None);

        accessGranter.GrantedAccesses.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_AccessDenied_WhenUserNotFamilyMember_ThrowsFamilyException()
    {
        var (db, family, _) = await BuildFamilyWithManagerAsync();
        var auth = new StubFamilyAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth: auth);

        var act = () => handler.Handle(
            new InviteMemberCommand(
                family.Id.Value,
                "Dave",
                "Adult",
                null,
                false,
                "dave@household.local",
                "Password123",
                Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_AccessDenied_WhenRequestingUserIsNotManager_ThrowsFamilyException()
    {
        var db = CreateDb();
        var nonManagerUserId = Guid.NewGuid();

        var family = Domain.Family.Family.Create(
            FamilyId.New(),
            FamilyName.Create("Test Family"),
            null,
            DateTime.UtcNow);

        // Add as non-manager member
        family.AddMember(
            MemberId.New(),
            MemberName.Create("Regular"),
            MemberRole.Adult,
            isManager: false,
            birthDate: null,
            DateTime.UtcNow,
            authUserId: nonManagerUserId);

        db.Set<Domain.Family.Family>().Add(family);
        await db.SaveChangesAsync();
        family.ClearDomainEvents();

        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new InviteMemberCommand(
                family.Id.Value,
                "Eve",
                "Adult",
                null,
                false,
                "eve@household.local",
                "Password123",
                nonManagerUserId),
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
            new InviteMemberCommand(
                Guid.NewGuid(),
                "Frank",
                "Adult",
                null,
                false,
                "frank@household.local",
                "Password123",
                Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.FamilyNotFound);
    }

    [Fact]
    public async Task Handle_DuplicateUsername_ThrowsFamilyException()
    {
        var (db, family, managerUserId) = await BuildFamilyWithManagerAsync();
        var userRepo = new InMemoryAuthUserRepository(
            [new AuthUserRecord(Guid.NewGuid(), "taken@household.local", "hash")]);
        var handler = BuildHandler(db, userRepo: userRepo);

        var act = () => handler.Handle(
            new InviteMemberCommand(
                family.Id.Value,
                "Grace",
                "Adult",
                null,
                false,
                "taken@household.local",
                "Password123",
                managerUserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.InvalidInput);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyName_ThrowsFamilyException(string name)
    {
        var (db, family, managerUserId) = await BuildFamilyWithManagerAsync();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new InviteMemberCommand(
                family.Id.Value,
                name,
                "Adult",
                null,
                false,
                "test@household.local",
                "Password123",
                managerUserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_WeakTemporaryPassword_ThrowsFamilyException()
    {
        var (db, family, managerUserId) = await BuildFamilyWithManagerAsync();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new InviteMemberCommand(
                family.Id.Value,
                "Henry",
                "Adult",
                null,
                false,
                "henry@household.local",
                "short",
                managerUserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_WithBirthDateAndManagerFlag_SetsMemberCorrectly()
    {
        var (db, family, managerUserId) = await BuildFamilyWithManagerAsync();
        var handler = BuildHandler(db);

        var birthDate = new DateOnly(1990, 5, 15);
        var result = await handler.Handle(
            new InviteMemberCommand(
                family.Id.Value,
                "Ivy",
                "Adult",
                birthDate,
                true,
                "ivy@household.local",
                "Password123",
                managerUserId),
            CancellationToken.None);

        result.BirthDate.Should().Be(birthDate);
        result.IsManager.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ManagerFlagOnChildRole_ThrowsFamilyException()
    {
        var (db, family, managerUserId) = await BuildFamilyWithManagerAsync();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new InviteMemberCommand(
                family.Id.Value,
                "Jack",
                "Child",
                null,
                true, // manager=true but role=Child is invalid
                "jack@household.local",
                "Password123",
                managerUserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.InvalidInput);
    }
}
