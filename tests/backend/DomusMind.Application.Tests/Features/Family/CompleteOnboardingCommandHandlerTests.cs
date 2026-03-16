using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.CompleteOnboarding;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Family;

public sealed class CompleteOnboardingCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

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

    private static CompleteOnboardingCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubFamilyAuthorizationService? auth = null)
        => new(db, new EventLogWriter(db), auth ?? new StubFamilyAuthorizationService());

    [Fact]
    public async Task Handle_CreatesCreatorMemberAsManager()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var userId = Guid.NewGuid();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new CompleteOnboardingCommand(
                family.Id.Value, userId, "Juan", null, []),
            CancellationToken.None);

        result.Members.Should().HaveCount(1);
        result.Members.Single().Name.Should().Be("Juan");
        result.Members.Single().Role.Should().Be("Adult");
        result.Members.Single().IsManager.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_PersistsCreatorToDatabaseAsManager()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var handler = BuildHandler(db);

        await handler.Handle(
            new CompleteOnboardingCommand(
                family.Id.Value, Guid.NewGuid(), "Ana", null, []),
            CancellationToken.None);

        var members = await db.Set<FamilyMember>().ToListAsync();
        members.Should().HaveCount(1);
        members.Single().IsManager.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AddsAdditionalMembers()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var handler = BuildHandler(db);

        var additional = new List<AdditionalMemberInput>
        {
            new("Gema", null, "adult", true),
            new("Julia", null, "child", false),
        };

        var result = await handler.Handle(
            new CompleteOnboardingCommand(
                family.Id.Value, Guid.NewGuid(), "Juan", null, additional),
            CancellationToken.None);

        result.Members.Should().HaveCount(3);
        result.Members.Should().Contain(m => m.Name == "Gema" && m.IsManager);
        result.Members.Should().Contain(m => m.Name == "Julia" && !m.IsManager && m.Role == "Child");
    }

    [Fact]
    public async Task Handle_WithPetType_AddsMemberWithPetRole()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new CompleteOnboardingCommand(
                family.Id.Value, Guid.NewGuid(), "Juan", null,
                [new AdditionalMemberInput("Max", null, "pet", false)]),
            CancellationToken.None);

        result.Members.Should().Contain(m => m.Name == "Max" && m.Role == "Pet");
    }

    [Fact]
    public async Task Handle_WithManagerOnChild_ThrowsFamilyException()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new CompleteOnboardingCommand(
                family.Id.Value, Guid.NewGuid(), "Juan", null,
                [new AdditionalMemberInput("Julia", null, "child", true)]),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_WithManagerOnPet_ThrowsFamilyException()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new CompleteOnboardingCommand(
                family.Id.Value, Guid.NewGuid(), "Juan", null,
                [new AdditionalMemberInput("Max", null, "pet", true)]),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_WithEmptySelfName_ThrowsFamilyException()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new CompleteOnboardingCommand(
                family.Id.Value, Guid.NewGuid(), "   ", null, []),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsFamilyException()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var auth = new StubFamilyAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new CompleteOnboardingCommand(
                family.Id.Value, Guid.NewGuid(), "Juan", null, []),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_WithBirthDate_PersistsBirthDate()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var handler = BuildHandler(db);
        var birthDate = new DateOnly(1985, 4, 12);

        var result = await handler.Handle(
            new CompleteOnboardingCommand(
                family.Id.Value, Guid.NewGuid(), "Juan", birthDate, []),
            CancellationToken.None);

        result.Members.Single().BirthDate.Should().Be(birthDate);
    }
}
