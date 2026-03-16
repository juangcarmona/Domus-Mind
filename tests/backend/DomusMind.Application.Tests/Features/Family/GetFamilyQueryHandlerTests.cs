using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.GetFamily;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Family;

public sealed class GetFamilyQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetFamilyQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubFamilyAuthorizationService? auth = null)
        => new(db, auth ?? new StubFamilyAuthorizationService());

    private static async Task<(DomusMindDbContext Db, Domain.Family.Family Family)> BuildWithFamilyAsync(
        string name = "Test Family")
    {
        var db = CreateDb();
        var family = Domain.Family.Family.Create(
            FamilyId.New(),
            FamilyName.Create(name),
            null,
            DateTime.UtcNow);
        db.Set<Domain.Family.Family>().Add(family);
        await db.SaveChangesAsync();
        return (db, family);
    }

    [Fact]
    public async Task Handle_ExistingFamily_ReturnsFamilyResponse()
    {
        var (db, family) = await BuildWithFamilyAsync("Johnson Family");
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetFamilyQuery(family.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.FamilyId.Should().Be(family.Id.Value);
        result.Name.Should().Be("Johnson Family");
        result.MemberCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_FamilyWithMembers_ReturnsCorrectMemberCount()
    {
        var (db, family) = await BuildWithFamilyAsync();
        // Re-load and add a member via AddMember handler pattern
        var loaded = await db.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleAsync(f => f.Id == family.Id);
        loaded.AddMember(MemberId.New(), MemberName.Create("Alice"), MemberRole.Adult, DateTime.UtcNow);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var result = await handler.Handle(
            new GetFamilyQuery(family.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.MemberCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsFamilyException()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var auth = new StubFamilyAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new GetFamilyQuery(family.Id.Value, Guid.NewGuid()),
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
            new GetFamilyQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.FamilyNotFound);
    }
}
