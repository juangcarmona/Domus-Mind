using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.UpdateMemberProfile;
using DomusMind.Application.Tests.Features.Auth;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Family;

public sealed class UpdateMemberProfileCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static UpdateMemberProfileCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubFamilyAuthorizationService? auth = null)
        => new(db, new StubEventLogWriter(), auth ?? new StubFamilyAuthorizationService());

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
    public async Task Handle_ManagerCanUpdateAnyMemberProfile()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var managerUserId = Guid.NewGuid();
        var targetMemberId = MemberId.New();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(MemberId.New(), MemberName.Create("Manager"), MemberRole.Adult,
            isManager: true, birthDate: null, DateTime.UtcNow, authUserId: managerUserId);
        loaded.AddMember(targetMemberId, MemberName.Create("Alice"), MemberRole.Adult, DateTime.UtcNow);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var result = await handler.Handle(
            new UpdateMemberProfileCommand(
                family.Id.Value, targetMemberId.Value, managerUserId,
                "Ali", "+1-555-0100", "ali@example.com", "Friendly"),
            CancellationToken.None);

        result.PreferredName.Should().Be("Ali");
        result.PrimaryPhone.Should().Be("+1-555-0100");
        result.PrimaryEmail.Should().Be("ali@example.com");
        result.HouseholdNote.Should().Be("Friendly");
    }

    [Fact]
    public async Task Handle_MemberCanUpdateOwnProfile()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var selfUserId = Guid.NewGuid();
        var selfMemberId = MemberId.New();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(selfMemberId, MemberName.Create("Bob"), MemberRole.Adult,
            isManager: false, birthDate: null, DateTime.UtcNow, authUserId: selfUserId);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var result = await handler.Handle(
            new UpdateMemberProfileCommand(
                family.Id.Value, selfMemberId.Value, selfUserId,
                "Bobby", null, null, null),
            CancellationToken.None);

        result.PreferredName.Should().Be("Bobby");
    }

    [Fact]
    public async Task Handle_NonManagerCannotUpdateOtherMember()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var nonManagerUserId = Guid.NewGuid();
        var targetMemberId = MemberId.New();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(MemberId.New(), MemberName.Create("NonManager"), MemberRole.Adult,
            isManager: false, birthDate: null, DateTime.UtcNow, authUserId: nonManagerUserId);
        loaded.AddMember(targetMemberId, MemberName.Create("Target"), MemberRole.Adult, DateTime.UtcNow);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var act = () => handler.Handle(
            new UpdateMemberProfileCommand(
                family.Id.Value, targetMemberId.Value, nonManagerUserId,
                "T", null, null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_MemberNotFound_ThrowsFamilyException()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var managerUserId = Guid.NewGuid();

        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(MemberId.New(), MemberName.Create("Manager"), MemberRole.Adult,
            isManager: true, birthDate: null, DateTime.UtcNow, authUserId: managerUserId);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var act = () => handler.Handle(
            new UpdateMemberProfileCommand(
                family.Id.Value, Guid.NewGuid(), managerUserId,
                null, null, null, null),
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
            new UpdateMemberProfileCommand(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                null, null, null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.FamilyNotFound);
    }
}
