using DomusMind.Application.Features.Lists;
using DomusMind.Application.Features.Lists.UpdateListItem;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Lists;

public sealed class UpdateListItemCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static UpdateListItemCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new UpdateListItemCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubListAuthorizationService());
    }

    private static async Task<(DomusMindDbContext db, SharedList list, ListItemId itemId)> SeedListWithItemAsync()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = ListTestHelpers.MakeList(familyId);
        var itemId = ListItemId.New();
        list.AddItem(itemId, ListItemName.Create("Original Name"), null, null, DateTime.UtcNow);
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();
        return (db, list, itemId);
    }

    [Fact]
    public async Task Handle_UpdatesItemName()
    {
        var (db, list, itemId) = await SeedListWithItemAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new UpdateListItemCommand(list.Id.Value, itemId.Value, "Updated Name", null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Handle_UpdatesQuantityAndNote()
    {
        var (db, list, itemId) = await SeedListWithItemAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new UpdateListItemCommand(list.Id.Value, itemId.Value, "Milk", "2L", "full-fat", Guid.NewGuid()),
            CancellationToken.None);

        result.Quantity.Should().Be("2L");
        result.Note.Should().Be("full-fat");
    }

    [Fact]
    public async Task Handle_WithEmptyName_ThrowsListException()
    {
        var (db, list, itemId) = await SeedListWithItemAsync();
        var handler = BuildHandler(db: db);

        var act = async () => await handler.Handle(
            new UpdateListItemCommand(list.Id.Value, itemId.Value, "   ", null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(ex => ex.Code == ListErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_WithNameExceedingMaxLength_ThrowsListException()
    {
        var (db, list, itemId) = await SeedListWithItemAsync();
        var handler = BuildHandler(db: db);
        var longName = new string('x', ListItemName.MaxLength + 1);

        var act = async () => await handler.Handle(
            new UpdateListItemCommand(list.Id.Value, itemId.Value, longName, null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(ex => ex.Code == ListErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_WithUnknownItemId_ThrowsItemNotFound()
    {
        var (db, list, _) = await SeedListWithItemAsync();
        var handler = BuildHandler(db: db);

        var act = async () => await handler.Handle(
            new UpdateListItemCommand(list.Id.Value, Guid.NewGuid(), "Name", null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(ex => ex.Code == ListErrorCode.ItemNotFound);
    }

    [Fact]
    public async Task Handle_WithUnknownListId_ThrowsListNotFound()
    {
        var handler = BuildHandler();

        var act = async () => await handler.Handle(
            new UpdateListItemCommand(Guid.NewGuid(), Guid.NewGuid(), "Name", null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(ex => ex.Code == ListErrorCode.ListNotFound);
    }

    [Fact]
    public async Task Handle_PersistsChangeToDB()
    {
        var (db, list, itemId) = await SeedListWithItemAsync();
        var handler = BuildHandler(db: db);

        await handler.Handle(
            new UpdateListItemCommand(list.Id.Value, itemId.Value, "Persisted Name", null, null, Guid.NewGuid()),
            CancellationToken.None);

        var reloaded = await db.Set<SharedList>()
            .Include(l => l.Items)
            .SingleAsync(l => l.Id == list.Id);
        reloaded.Items.Should().Contain(i => i.Name.Value == "Persisted Name");
    }
}
