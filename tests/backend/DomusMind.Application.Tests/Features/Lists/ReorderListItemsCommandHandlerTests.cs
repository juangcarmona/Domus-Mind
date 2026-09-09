using DomusMind.Application.Features.Lists;
using DomusMind.Application.Features.Lists.ReorderListItems;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Lists;

public sealed class ReorderListItemsCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ReorderListItemsCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new ReorderListItemsCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubListAuthorizationService());
    }

    private static async Task<(DomusMindDbContext db, SharedList list, ListItemId id1, ListItemId id2, ListItemId id3)>
        SeedListWithThreeItemsAsync()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = ListTestHelpers.MakeList(familyId);
        var id1 = ListItemId.New();
        var id2 = ListItemId.New();
        var id3 = ListItemId.New();
        list.AddItem(id1, ListItemName.Create("Alpha"), null, null, DateTime.UtcNow);
        list.AddItem(id2, ListItemName.Create("Beta"), null, null, DateTime.UtcNow);
        list.AddItem(id3, ListItemName.Create("Gamma"), null, null, DateTime.UtcNow);
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();
        return (db, list, id1, id2, id3);
    }

    [Fact]
    public async Task Handle_ReordersUncheckedItemsSuccessfully()
    {
        var (db, list, id1, id2, id3) = await SeedListWithThreeItemsAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new ReorderListItemsCommand(
                list.Id.Value, [id3.Value, id1.Value, id2.Value], Guid.NewGuid()),
            CancellationToken.None);

        result.Should().BeTrue();
        var persisted = await db.Set<SharedList>().Include(l => l.Items)
            .SingleAsync(l => l.Id == list.Id);
        persisted.Items.Single(i => i.Id == id3).Order.Should().Be(1);
        persisted.Items.Single(i => i.Id == id1).Order.Should().Be(2);
        persisted.Items.Single(i => i.Id == id2).Order.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithEmptyPayload_ThrowsListException()
    {
        var (db, list, _, _, _) = await SeedListWithThreeItemsAsync();
        var handler = BuildHandler(db: db);

        var act = async () => await handler.Handle(
            new ReorderListItemsCommand(list.Id.Value, [], Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(ex => ex.Code == ListErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_WithWrongItemCount_ThrowsInvalidInput()
    {
        var (db, list, id1, _, _) = await SeedListWithThreeItemsAsync();
        var handler = BuildHandler(db: db);

        // Only 1 ID provided but list has 3 unchecked items
        var act = async () => await handler.Handle(
            new ReorderListItemsCommand(list.Id.Value, [id1.Value], Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(ex => ex.Code == ListErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_WithDuplicateItemIds_ThrowsInvalidInput()
    {
        var (db, list, id1, _, _) = await SeedListWithThreeItemsAsync();
        var handler = BuildHandler(db: db);

        var act = async () => await handler.Handle(
            new ReorderListItemsCommand(list.Id.Value, [id1.Value, id1.Value, id1.Value], Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(ex => ex.Code == ListErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_WithUnknownListId_ThrowsListNotFound()
    {
        var db = CreateDb();
        var handler = BuildHandler(db: db);

        var act = async () => await handler.Handle(
            new ReorderListItemsCommand(Guid.NewGuid(), [Guid.NewGuid()], Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(ex => ex.Code == ListErrorCode.ListNotFound);
    }

    [Fact]
    public async Task Handle_OrderRemainsStableAfterCheckedItemExcluded()
    {
        var (db, list, id1, id2, id3) = await SeedListWithThreeItemsAsync();
        // Check id2 - id1 and id3 are unchecked; full reorder includes all 3
        list.ToggleItem(id2, null, DateTime.UtcNow);
        list.ClearDomainEvents();
        await db.SaveChangesAsync();

        var handler = BuildHandler(db: db);
        // Caller must pass all items; checked id2 is included at its desired position
        await handler.Handle(
            new ReorderListItemsCommand(list.Id.Value, [id3.Value, id2.Value, id1.Value], Guid.NewGuid()),
            CancellationToken.None);

        var persisted = await db.Set<SharedList>().Include(l => l.Items)
            .SingleAsync(l => l.Id == list.Id);
        persisted.Items.Single(i => i.Id == id3).Order.Should().Be(1);
        persisted.Items.Single(i => i.Id == id2).Order.Should().Be(2);
        persisted.Items.Single(i => i.Id == id1).Order.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithAccessDenied_ThrowsListException()
    {
        var (db, list, id1, id2, id3) = await SeedListWithThreeItemsAsync();
        var handler = BuildHandler(db: db, auth: new StubListAuthorizationService { CanAccess = false });

        var act = async () => await handler.Handle(
            new ReorderListItemsCommand(list.Id.Value, [id1.Value, id2.Value, id3.Value], Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(ex => ex.Code == ListErrorCode.AccessDenied);
    }
}
