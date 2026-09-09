using DomusMind.Application.Features.Lists;
using DomusMind.Application.Features.Lists.GetListDetail;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Lists;

public sealed class GetListDetailQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetListDetailQueryHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new GetListDetailQueryHandler(
            context, auth ?? new StubListAuthorizationService());
    }

    private static async Task<(DomusMindDbContext db, SharedList list)> SeedListAsync(
        Action<SharedList>? configure = null)
    {
        var db = CreateDb();
        var list = ListTestHelpers.MakeList(FamilyId.New());
        configure?.Invoke(list);
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();
        return (db, list);
    }

    [Fact]
    public async Task Handle_ReturnsListMetadata()
    {
        var (db, list) = await SeedListAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetListDetailQuery(list.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.ListId.Should().Be(list.Id.Value);
        result.Name.Should().Be(list.Name.Value);
        result.Kind.Should().Be(list.Kind.Value);
    }

    [Fact]
    public async Task Handle_ReturnsOrderedItems()
    {
        var (db, list) = await SeedListAsync(l =>
        {
            l.AddItem(ListItemId.New(), ListItemName.Create("Apples"), null, null, DateTime.UtcNow);
            l.AddItem(ListItemId.New(), ListItemName.Create("Bananas"), null, null, DateTime.UtcNow);
            l.AddItem(ListItemId.New(), ListItemName.Create("Cherries"), null, null, DateTime.UtcNow);
        });
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetListDetailQuery(list.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Items.Should().HaveCount(3);
        result.Items.Select(i => i.Order).Should().BeInAscendingOrder();
        result.Items[0].Name.Should().Be("Apples");
        result.Items[1].Name.Should().Be("Bananas");
        result.Items[2].Name.Should().Be("Cherries");
    }

    [Fact]
    public async Task Handle_ItemsIncludeCheckedState()
    {
        ListItemId itemId = default;
        var (db, list) = await SeedListAsync(l =>
        {
            itemId = ListItemId.New();
            l.AddItem(itemId, ListItemName.Create("Milk"), null, null, DateTime.UtcNow);
            l.ToggleItem(itemId, null, DateTime.UtcNow);
        });
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetListDetailQuery(list.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Items.Single().Checked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ItemsIncludeQuantityAndNote()
    {
        var (db, list) = await SeedListAsync(l =>
        {
            l.AddItem(ListItemId.New(), ListItemName.Create("Flour"), "1kg", "whole wheat", DateTime.UtcNow);
        });
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetListDetailQuery(list.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        var item = result.Items.Single();
        item.Quantity.Should().Be("1kg");
        item.Note.Should().Be("whole wheat");
    }

    [Fact]
    public async Task Handle_EmptyList_ReturnsEmptyItems()
    {
        var (db, list) = await SeedListAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetListDetailQuery(list.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ListNotFound_ThrowsListException()
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new GetListDetailQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.ListNotFound);
    }

    [Fact]
    public async Task Handle_WhenAccessDenied_ThrowsListException()
    {
        var (db, list) = await SeedListAsync();
        var auth = new StubListAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db: db, auth: auth);

        var act = () => handler.Handle(
            new GetListDetailQuery(list.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.AccessDenied);
    }
}
