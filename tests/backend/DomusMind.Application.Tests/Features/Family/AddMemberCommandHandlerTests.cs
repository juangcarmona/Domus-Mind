using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.AddMember;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Family;

public sealed class AddMemberCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static AddMemberCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubFamilyAuthorizationService? auth = null)
        => new(db, new EventLogWriter(db), auth ?? new StubFamilyAuthorizationService());

    private static async Task<(DomusMindDbContext Db, Domain.Family.Family Family)> BuildWithFamilyAsync()
    {
        var db = CreateDb();
        var family = Domain.Family.Family.Create(
            FamilyId.New(),
            FamilyName.Create("Test Family"),
            DateTime.UtcNow);
        db.Set<Domain.Family.Family>().Add(family);
        await db.SaveChangesAsync();
        return (db, family);
    }

    [Fact]
    public async Task Handle_WithValidInput_ReturnsMemberResponse()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var userId = Guid.NewGuid();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new AddMemberCommand(family.Id.Value, "Alice", "Adult", userId),
            CancellationToken.None);

        result.MemberId.Should().NotBeEmpty();
        result.FamilyId.Should().Be(family.Id.Value);
        result.Name.Should().Be("Alice");
        result.Role.Should().Be("Adult");
    }

    [Fact]
    public async Task Handle_PersistsMemberToDatabase()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var userId = Guid.NewGuid();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new AddMemberCommand(family.Id.Value, "Bob", "Child", userId),
            CancellationToken.None);

        var saved = await db.Set<FamilyMember>().FindAsync(MemberId.From(result.MemberId));
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsFamilyException()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var auth = new StubFamilyAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new AddMemberCommand(family.Id.Value, "Charlie", "Adult", Guid.NewGuid()),
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
            new AddMemberCommand(Guid.NewGuid(), "Dave", "Adult", Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.FamilyNotFound);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyMemberName_ThrowsFamilyException(string name)
    {
        var (db, family) = await BuildWithFamilyAsync();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new AddMemberCommand(family.Id.Value, name, "Adult", Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_InvalidRole_ThrowsArgumentException()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new AddMemberCommand(family.Id.Value, "Eve", "Owner", Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
