using DomusMind.Application.Features.Lists;
using DomusMind.Application.Features.Lists.GetFamilyLists;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Lists;

public sealed class GetFamilyListsQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetFamilyListsQueryHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new GetFamilyListsQueryHandler(
            context, auth ?? new StubListAuthorizationService());
    }

    [Fact]
    public async Task Handle_ReturnsSummariesForFamily()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = ListTestHelpers.MakeList(familyId, "Groceries");
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetFamilyListsQuery(familyId.Value, Guid.NewGuid()),
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
        db.Set<SharedList>().Add(ListTestHelpers.MakeList(familyA, "Family A List"));
        var familyBList = ListTestHelpers.MakeList(familyB, "Family B List");
        db.Set<SharedList>().Add(familyBList);
        foreach (var l in new[] { db.Set<SharedList>().Local.First(), db.Set<SharedList>().Local.Skip(1).First() })
            l.ClearDomainEvents();
        await db.SaveChangesAsync();

        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetFamilyListsQuery(familyA.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Lists.Should().HaveCount(1);
        result.Lists[0].Name.Should().Be("Family A List");
    }

    [Fact]
    public async Task Handle_ReturnsCorrectItemCountAndUncheckedCount()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = ListTestHelpers.MakeList(familyId, "Shopping");
        var id1 = ListItemId.New();
        var id2 = ListItemId.New();
        list.AddItem(id1, ListItemName.Create("Apples"), null, null, DateTime.UtcNow);
        list.AddItem(id2, ListItemName.Create("Bananas"), null, null, DateTime.UtcNow);
        list.ToggleItem(id1, null, DateTime.UtcNow);
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetFamilyListsQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        var summary = result.Lists[0];
        summary.UncheckedCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_EmptyFamily_ReturnsEmptyList()
    {
        var handler = BuildHandler();

        var result = await handler.Handle(
            new GetFamilyListsQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.Lists.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenAccessDenied_ThrowsListException()
    {
        var auth = new StubListAuthorizationService { CanAccess = false };
        var handler = BuildHandler(auth: auth);

        var act = () => handler.Handle(
            new GetFamilyListsQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_ReturnsSummaryWithKindAndAreaId()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var areaId = Guid.NewGuid();
        var list = SharedList.Create(
            ListId.New(), familyId,
            ListName.Create("Cleaning Supplies"), ListKind.Create("Household"),
            Domain.Responsibilities.ResponsibilityDomainId.From(areaId),
            null, null, DateTime.UtcNow);
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetFamilyListsQuery(familyId.Value, Guid.NewGuid()),
            CancellationToken.None);

        var summary = result.Lists[0];
        summary.Kind.Should().Be("Household");
        summary.AreaId.Should().Be(areaId);
    }
}
