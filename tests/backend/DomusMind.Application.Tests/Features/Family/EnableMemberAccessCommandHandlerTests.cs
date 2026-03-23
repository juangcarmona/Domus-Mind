using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.EnableMemberAccess;
using DomusMind.Application.Tests.Features.Auth;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Family;

public sealed class EnableMemberAccessCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static EnableMemberAccessCommandHandler BuildHandler(
        DomusMindDbContext db,
        InMemoryAuthUserRepository? users = null,
        StubFamilyAuthorizationService? auth = null)
        => new(
            db,
            auth ?? new StubFamilyAuthorizationService(),
            users ?? new InMemoryAuthUserRepository());

    private static async Task<(DomusMindDbContext Db, Domain.Family.Family Family, Guid ManagerUserId, Guid TargetUserId, Guid TargetMemberId)>
        BuildFamilyWithLinkedMemberAsync(bool targetDisabled = true)
    {
        var db = CreateDb();
        var managerUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
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
            MemberName.Create("Target"),
            MemberRole.Adult,
            isManager: false,
            birthDate: null,
            DateTime.UtcNow,
            authUserId: targetUserId);

        db.Set<Domain.Family.Family>().Add(family);
        await db.SaveChangesAsync();
        family.ClearDomainEvents();
        return (db, family, managerUserId, targetUserId, targetMemberId.Value);
    }

    [Fact]
    public async Task Handle_EnablesDisabledAuthUser()
    {
        var (db, family, managerUserId, targetUserId, targetMemberId) = await BuildFamilyWithLinkedMemberAsync();
        var userRepo = new InMemoryAuthUserRepository(
        [
            new AuthUserRecord(targetUserId, "target@home.local", "HASHED:pw", IsDisabled: true),
        ]);
        var handler = BuildHandler(db, users: userRepo);

        var result = await handler.Handle(
            new EnableMemberAccessCommand(family.Id.Value, targetMemberId, managerUserId),
            CancellationToken.None);

        result.MemberId.Should().Be(targetMemberId);
        userRepo.Users.Single(u => u.UserId == targetUserId).IsDisabled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NonManager_ThrowsAccessDenied()
    {
        var (db, family, _, _, targetMemberId) = await BuildFamilyWithLinkedMemberAsync();
        var nonManagerUserId = Guid.NewGuid();

        // Add non-manager member for the requester
        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(MemberId.New(), MemberName.Create("Regular"), MemberRole.Adult,
            isManager: false, birthDate: null, DateTime.UtcNow, authUserId: nonManagerUserId);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new EnableMemberAccessCommand(family.Id.Value, targetMemberId, nonManagerUserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_MemberWithNoLinkedAccount_ThrowsInvalidInput()
    {
        var (db, family) = await BuildFamilyWithCleanSeedAsync();
        var managerUserId = Guid.NewGuid();
        var noAccountMemberId = MemberId.New();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(MemberId.New(), MemberName.Create("Manager"), MemberRole.Adult,
            isManager: true, birthDate: null, DateTime.UtcNow, authUserId: managerUserId);
        loaded.AddMember(noAccountMemberId, MemberName.Create("NoAccount"), MemberRole.Adult, DateTime.UtcNow);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new EnableMemberAccessCommand(family.Id.Value, noAccountMemberId.Value, managerUserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_FamilyNotFound_ThrowsFamilyException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new EnableMemberAccessCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.FamilyNotFound);
    }

    // Helper: builds a family without any members
    private static async Task<(DomusMindDbContext Db, Domain.Family.Family Family)> BuildFamilyWithCleanSeedAsync()
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
}
