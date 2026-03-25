using DomusMind.Application.Features.SharedLists;
using DomusMind.Application.Features.SharedLists.GetSharedListDetail;
using DomusMind.Domain.Family;
using DomusMind.Domain.SharedLists;
using DomusMind.Domain.SharedLists.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.SharedLists;

public sealed class GetSharedListDetailQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetSharedListDetailQueryHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubSharedListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new GetSharedListDetailQueryHandler(
            context, auth ?? new StubSharedListAuthorizationService());
    }

    private static async Task<(DomusMindDbContext db, SharedList list)> SeedListAsync(
        Action<SharedList>? configure = null)
    {
        var db = CreateDb();
        var list = SharedListTestHelpers.MakeList(FamilyId.New());
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
            new GetSharedListDetailQuery(list.Id.Value, Guid.NewGuid()),
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
            l.AddItem(SharedListItemId.New(), SharedListItemName.Create("Apples"), null, null, DateTime.UtcNow);
            l.AddItem(SharedListItemId.New(), SharedListItemName.Create("Bananas"), null, null, DateTime.UtcNow);
            l.AddItem(SharedListItemId.New(), SharedListItemName.Create("Cherries"), null, null, DateTime.UtcNow);
        });
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetSharedListDetailQuery(list.Id.Value, Guid.NewGuid()),
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
        SharedListItemId itemId = default;
        var (db, list) = await SeedListAsync(l =>
        {
            itemId = SharedListItemId.New();
            l.AddItem(itemId, SharedListItemName.Create("Milk"), null, null, DateTime.UtcNow);
            l.ToggleItem(itemId, null, DateTime.UtcNow);
        });
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetSharedListDetailQuery(list.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Items.Single().Checked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ItemsIncludeQuantityAndNote()
    {
        var (db, list) = await SeedListAsync(l =>
        {
            l.AddItem(SharedListItemId.New(), SharedListItemName.Create("Flour"), "1kg", "whole wheat", DateTime.UtcNow);
        });
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetSharedListDetailQuery(list.Id.Value, Guid.NewGuid()),
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
            new GetSharedListDetailQuery(list.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ListNotFound_ThrowsSharedListException()
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new GetSharedListDetailQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(e => e.Code == SharedListErrorCode.ListNotFound);
    }

    [Fact]
    public async Task Handle_WhenAccessDenied_ThrowsSharedListException()
    {
        var (db, list) = await SeedListAsync();
        var auth = new StubSharedListAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db: db, auth: auth);

        var act = () => handler.Handle(
            new GetSharedListDetailQuery(list.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(e => e.Code == SharedListErrorCode.AccessDenied);
    }
}
