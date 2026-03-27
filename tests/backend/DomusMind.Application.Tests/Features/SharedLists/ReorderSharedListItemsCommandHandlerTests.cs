using DomusMind.Application.Features.SharedLists;
using DomusMind.Application.Features.SharedLists.ReorderSharedListItems;
using DomusMind.Domain.Family;
using DomusMind.Domain.SharedLists;
using DomusMind.Domain.SharedLists.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.SharedLists;

public sealed class ReorderSharedListItemsCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ReorderSharedListItemsCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubSharedListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new ReorderSharedListItemsCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubSharedListAuthorizationService());
    }

    private static async Task<(DomusMindDbContext db, SharedList list, SharedListItemId id1, SharedListItemId id2, SharedListItemId id3)>
        SeedListWithThreeItemsAsync()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = SharedListTestHelpers.MakeList(familyId);
        var id1 = SharedListItemId.New();
        var id2 = SharedListItemId.New();
        var id3 = SharedListItemId.New();
        list.AddItem(id1, SharedListItemName.Create("Alpha"), null, null, DateTime.UtcNow);
        list.AddItem(id2, SharedListItemName.Create("Beta"), null, null, DateTime.UtcNow);
        list.AddItem(id3, SharedListItemName.Create("Gamma"), null, null, DateTime.UtcNow);
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
            new ReorderSharedListItemsCommand(
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
    public async Task Handle_WithEmptyPayload_ThrowsSharedListException()
    {
        var (db, list, _, _, _) = await SeedListWithThreeItemsAsync();
        var handler = BuildHandler(db: db);

        var act = async () => await handler.Handle(
            new ReorderSharedListItemsCommand(list.Id.Value, [], Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(ex => ex.Code == SharedListErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_WithWrongItemCount_ThrowsInvalidInput()
    {
        var (db, list, id1, _, _) = await SeedListWithThreeItemsAsync();
        var handler = BuildHandler(db: db);

        // Only 1 ID provided but list has 3 unchecked items
        var act = async () => await handler.Handle(
            new ReorderSharedListItemsCommand(list.Id.Value, [id1.Value], Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(ex => ex.Code == SharedListErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_WithDuplicateItemIds_ThrowsInvalidInput()
    {
        var (db, list, id1, _, _) = await SeedListWithThreeItemsAsync();
        var handler = BuildHandler(db: db);

        var act = async () => await handler.Handle(
            new ReorderSharedListItemsCommand(list.Id.Value, [id1.Value, id1.Value, id1.Value], Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(ex => ex.Code == SharedListErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_WithUnknownListId_ThrowsListNotFound()
    {
        var db = CreateDb();
        var handler = BuildHandler(db: db);

        var act = async () => await handler.Handle(
            new ReorderSharedListItemsCommand(Guid.NewGuid(), [Guid.NewGuid()], Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(ex => ex.Code == SharedListErrorCode.ListNotFound);
    }

    [Fact]
    public async Task Handle_OrderRemainsStableAfterCheckedItemExcluded()
    {
        var (db, list, id1, id2, id3) = await SeedListWithThreeItemsAsync();
        // Check id2 - only id1 and id3 remain unchecked
        list.ToggleItem(id2, null, DateTime.UtcNow);
        list.ClearDomainEvents();
        await db.SaveChangesAsync();

        var handler = BuildHandler(db: db);
        await handler.Handle(
            new ReorderSharedListItemsCommand(list.Id.Value, [id3.Value, id1.Value], Guid.NewGuid()),
            CancellationToken.None);

        var persisted = await db.Set<SharedList>().Include(l => l.Items)
            .SingleAsync(l => l.Id == list.Id);
        persisted.Items.Single(i => i.Id == id3).Order.Should().Be(1);
        persisted.Items.Single(i => i.Id == id1).Order.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithAccessDenied_ThrowsSharedListException()
    {
        var (db, list, id1, id2, id3) = await SeedListWithThreeItemsAsync();
        var handler = BuildHandler(db: db, auth: new StubSharedListAuthorizationService { CanAccess = false });

        var act = async () => await handler.Handle(
            new ReorderSharedListItemsCommand(list.Id.Value, [id1.Value, id2.Value, id3.Value], Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(ex => ex.Code == SharedListErrorCode.AccessDenied);
    }
}
