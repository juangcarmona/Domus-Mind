using DomusMind.Application.Features.SharedLists;
using DomusMind.Application.Features.SharedLists.GetFamilySharedLists;
using DomusMind.Domain.Family;
using DomusMind.Domain.SharedLists;
using DomusMind.Domain.SharedLists.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.SharedLists;

public sealed class GetFamilySharedListsQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetFamilySharedListsQueryHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubSharedListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new GetFamilySharedListsQueryHandler(
            context, auth ?? new StubSharedListAuthorizationService());
    }

    [Fact]
    public async Task Handle_ReturnsSummariesForFamily()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = SharedListTestHelpers.MakeList(familyId, "Groceries");
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetFamilySharedListsQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Lists.Should().HaveCount(1);
        result.Lists[0].Name.Should().Be("Groceries");
    }

    [Fact]
    public async Task Handle_DoesNotReturnListsOfOtherFamilies()
    {
        var db = CreateDb();
        var familyA = FamilyId.New();
        var familyB = FamilyId.New();
        db.Set<SharedList>().Add(SharedListTestHelpers.MakeList(familyA, "Family A List"));
        var familyBList = SharedListTestHelpers.MakeList(familyB, "Family B List");
        db.Set<SharedList>().Add(familyBList);
        foreach (var l in new[] { db.Set<SharedList>().Local.First(), db.Set<SharedList>().Local.Skip(1).First() })
            l.ClearDomainEvents();
        await db.SaveChangesAsync();

        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetFamilySharedListsQuery(familyA.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Lists.Should().HaveCount(1);
        result.Lists[0].Name.Should().Be("Family A List");
    }

    [Fact]
    public async Task Handle_ReturnsCorrectItemCountAndUncheckedCount()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = SharedListTestHelpers.MakeList(familyId, "Shopping");
        var id1 = SharedListItemId.New();
        var id2 = SharedListItemId.New();
        list.AddItem(id1, SharedListItemName.Create("Apples"), null, null, DateTime.UtcNow);
        list.AddItem(id2, SharedListItemName.Create("Bananas"), null, null, DateTime.UtcNow);
        list.ToggleItem(id1, null, DateTime.UtcNow);
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetFamilySharedListsQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        var summary = result.Lists[0];
        summary.ItemCount.Should().Be(2);
        summary.UncheckedCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_EmptyFamily_ReturnsEmptyList()
    {
        var handler = BuildHandler();

        var result = await handler.Handle(
            new GetFamilySharedListsQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.Lists.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenAccessDenied_ThrowsSharedListException()
    {
        var auth = new StubSharedListAuthorizationService { CanAccess = false };
        var handler = BuildHandler(auth: auth);

        var act = () => handler.Handle(
            new GetFamilySharedListsQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(e => e.Code == SharedListErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_ReturnsSummaryWithKindAndAreaId()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var areaId = Guid.NewGuid();
        var list = SharedList.Create(
            SharedListId.New(), familyId,
            SharedListName.Create("Cleaning Supplies"), SharedListKind.Create("Household"),
            Domain.Responsibilities.ResponsibilityDomainId.From(areaId),
            null, null, DateTime.UtcNow);
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetFamilySharedListsQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        var summary = result.Lists[0];
        summary.Kind.Should().Be("Household");
        summary.AreaId.Should().Be(areaId);
    }
}
