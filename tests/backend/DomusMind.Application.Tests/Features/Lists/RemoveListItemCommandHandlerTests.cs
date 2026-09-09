using DomusMind.Application.Features.Lists;
using DomusMind.Application.Features.Lists.RemoveListItem;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Lists;

public sealed class RemoveListItemCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static RemoveListItemCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new RemoveListItemCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubListAuthorizationService());
    }

    private static async Task<(DomusMindDbContext db, SharedList list, ListItemId itemId)> SeedListWithItemAsync()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = ListTestHelpers.MakeList(familyId);
        var itemId = ListItemId.New();
        list.AddItem(itemId, ListItemName.Create("Remove Me"), null, null, DateTime.UtcNow);
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();
        return (db, list, itemId);
    }

    [Fact]
    public async Task Handle_RemovesItemFromList()
    {
        var (db, list, itemId) = await SeedListWithItemAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new RemoveListItemCommand(list.Id.Value, itemId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Should().BeTrue();
        var reloaded = await db.Set<SharedList>()
            .Include(l => l.Items)
            .SingleAsync(l => l.Id == list.Id);
        reloaded.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithUnknownItemId_ThrowsItemNotFound()
    {
        var (db, list, _) = await SeedListWithItemAsync();
        var handler = BuildHandler(db: db);

        var act = async () => await handler.Handle(
            new RemoveListItemCommand(list.Id.Value, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(ex => ex.Code == ListErrorCode.ItemNotFound);
    }

    [Fact]
    public async Task Handle_WithUnknownListId_ThrowsListNotFound()
    {
        var handler = BuildHandler();

        var act = async () => await handler.Handle(
            new RemoveListItemCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(ex => ex.Code == ListErrorCode.ListNotFound);
    }

    [Fact]
    public async Task Handle_RemainingItemsNotAffected()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = ListTestHelpers.MakeList(familyId);
        var id1 = ListItemId.New();
        var id2 = ListItemId.New();
        list.AddItem(id1, ListItemName.Create("Keep"), null, null, DateTime.UtcNow);
        list.AddItem(id2, ListItemName.Create("Remove"), null, null, DateTime.UtcNow);
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db: db);

        await handler.Handle(
            new RemoveListItemCommand(list.Id.Value, id2.Value, Guid.NewGuid()),
            CancellationToken.None);

        var reloaded = await db.Set<SharedList>()
            .Include(l => l.Items)
            .SingleAsync(l => l.Id == list.Id);
        reloaded.Items.Should().HaveCount(1);
        reloaded.Items.Should().Contain(i => i.Name.Value == "Keep");
    }
}
