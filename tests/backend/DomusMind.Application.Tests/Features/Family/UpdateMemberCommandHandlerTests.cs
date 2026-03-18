using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.UpdateMember;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Family;

public sealed class UpdateMemberCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static UpdateMemberCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubFamilyAuthorizationService? auth = null)
        => new(db, new EventLogWriter(db), auth ?? new StubFamilyAuthorizationService());

    private static async Task<(DomusMindDbContext Db, Domain.Family.Family Family, Guid ManagerUserId, MemberId TargetMemberId)> BuildFamilyWithManagerAndMemberAsync()
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

        var targetMemberId = MemberId.New();
        family.AddMember(
            targetMemberId,
            MemberName.Create("Alice"),
            MemberRole.Adult,
            isManager: false,
            birthDate: null,
            DateTime.UtcNow);

        db.Set<Domain.Family.Family>().Add(family);
        await db.SaveChangesAsync();
        family.ClearDomainEvents();
        return (db, family, managerUserId, targetMemberId);
    }

    [Fact]
    public async Task Handle_WithValidInput_UpdatesMemberDetails()
    {
        var (db, family, managerUserId, targetMemberId) = await BuildFamilyWithManagerAndMemberAsync();
        var handler = BuildHandler(db);

        var birthDate = new DateOnly(1990, 3, 20);
        var result = await handler.Handle(
            new UpdateMemberCommand(
                family.Id.Value,
                targetMemberId.Value,
                "Alice Updated",
                "Adult",
                birthDate,
                false,
                managerUserId),
            CancellationToken.None);

        result.MemberId.Should().Be(targetMemberId.Value);
        result.Name.Should().Be("Alice Updated");
        result.BirthDate.Should().Be(birthDate);
        result.IsManager.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_PersistsChangesToDatabase()
    {
        var (db, family, managerUserId, targetMemberId) = await BuildFamilyWithManagerAndMemberAsync();
        var handler = BuildHandler(db);

        await handler.Handle(
            new UpdateMemberCommand(
                family.Id.Value,
                targetMemberId.Value,
                "Bob",
                "Child",
                null,
                false,
                managerUserId),
            CancellationToken.None);

        var saved = await db.Set<FamilyMember>().FindAsync(targetMemberId);
        saved.Should().NotBeNull();
        saved!.Name.Value.Should().Be("Bob");
        saved.Role.Value.Should().Be("Child");
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsFamilyException()
    {
        var (db, family, _, targetMemberId) = await BuildFamilyWithManagerAndMemberAsync();
        var auth = new StubFamilyAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new UpdateMemberCommand(
                family.Id.Value,
                targetMemberId.Value,
                "Alice",
                "Adult",
                null,
                false,
                Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_NonManagerUser_ThrowsFamilyException()
    {
        var db = CreateDb();
        var nonManagerUserId = Guid.NewGuid();

        var family = Domain.Family.Family.Create(
            FamilyId.New(),
            FamilyName.Create("Test Family"),
            null,
            DateTime.UtcNow);

        family.AddMember(
            MemberId.New(),
            MemberName.Create("Regular"),
            MemberRole.Adult,
            isManager: false,
            birthDate: null,
            DateTime.UtcNow,
            authUserId: nonManagerUserId);

        var targetMemberId = MemberId.New();
        family.AddMember(
            targetMemberId,
            MemberName.Create("Target"),
            MemberRole.Adult,
            isManager: false,
            birthDate: null,
            DateTime.UtcNow);

        db.Set<Domain.Family.Family>().Add(family);
        await db.SaveChangesAsync();
        family.ClearDomainEvents();

        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new UpdateMemberCommand(
                family.Id.Value,
                targetMemberId.Value,
                "Target Updated",
                "Adult",
                null,
                false,
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
            new UpdateMemberCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Alice",
                "Adult",
                null,
                false,
                Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.FamilyNotFound);
    }

    [Fact]
    public async Task Handle_MemberNotFound_ThrowsFamilyException()
    {
        var (db, family, managerUserId, _) = await BuildFamilyWithManagerAndMemberAsync();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new UpdateMemberCommand(
                family.Id.Value,
                Guid.NewGuid(), // non-existing member
                "Alice",
                "Adult",
                null,
                false,
                managerUserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.MemberNotFound);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyMemberName_ThrowsFamilyException(string name)
    {
        var (db, family, managerUserId, targetMemberId) = await BuildFamilyWithManagerAndMemberAsync();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new UpdateMemberCommand(
                family.Id.Value,
                targetMemberId.Value,
                name,
                "Adult",
                null,
                false,
                managerUserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_ManagerFlagOnChildRole_ThrowsFamilyException()
    {
        var (db, family, managerUserId, targetMemberId) = await BuildFamilyWithManagerAndMemberAsync();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new UpdateMemberCommand(
                family.Id.Value,
                targetMemberId.Value,
                "Alice",
                "Child",
                null,
                true, // manager=true but role=Child is invalid
                managerUserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.InvalidInput);
    }
}
