using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.GetMyFamily;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Family;

public sealed class GetMyFamilyQueryHandlerTests
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
            FamilyName.Create("Smith Family"),
            null,
            DateTime.UtcNow);
        db.Set<Domain.Family.Family>().Add(family);
        await db.SaveChangesAsync();
        return (db, family);
    }

    private static GetMyFamilyQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubUserFamilyAccessReader accessReader)
        => new(db, accessReader);

    [Fact]
    public async Task Handle_WhenUserHasFamily_ReturnsFamilyResponse()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var userId = Guid.NewGuid();
        var accessReader = new StubUserFamilyAccessReader(family.Id.Value);
        var handler = BuildHandler(db, accessReader);

        var result = await handler.Handle(
            new GetMyFamilyQuery(userId),
            CancellationToken.None);

        result.FamilyId.Should().Be(family.Id.Value);
        result.Name.Should().Be("Smith Family");
    }

    [Fact]
    public async Task Handle_WhenUserHasNoFamily_ThrowsFamilyNotFoundException()
    {
        var db = CreateDb();
        var accessReader = new StubUserFamilyAccessReader(null);
        var handler = BuildHandler(db, accessReader);

        var act = () => handler.Handle(
            new GetMyFamilyQuery(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FamilyException>()
            .Where(e => e.Code == FamilyErrorCode.FamilyNotFound);
    }

    [Fact]
    public async Task Handle_ReturnsMemberCount()
    {
        var (db, family) = await BuildWithFamilyAsync();
        var userId = Guid.NewGuid();

        family.AddMember(MemberId.New(), MemberName.Create("Ana"), MemberRole.Adult, true, null, DateTime.UtcNow);
        await db.SaveChangesAsync();

        var accessReader = new StubUserFamilyAccessReader(family.Id.Value);
        var handler = BuildHandler(db, accessReader);

        var result = await handler.Handle(
            new GetMyFamilyQuery(userId),
            CancellationToken.None);

        result.MemberCount.Should().Be(1);
    }
}
