using DomusMind.Application.Features.Lists;
using DomusMind.Application.Features.Lists.ToggleListItem;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Lists;

public sealed class ToggleListItemCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ToggleListItemCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new ToggleListItemCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubListAuthorizationService());
    }

    private static async Task<(DomusMindDbContext db, SharedList list, ListItemId itemId)> SeedListWithItemAsync()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = ListTestHelpers.MakeList(familyId);
        var itemId = ListItemId.New();
        list.AddItem(itemId, ListItemName.Create("Bread"), null, null, DateTime.UtcNow);
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();
        return (db, list, itemId);
    }

    [Fact]
    public async Task Handle_TogglesItemToChecked()
    {
        var (db, list, itemId) = await SeedListWithItemAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new ToggleListItemCommand(list.Id.Value, itemId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Checked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TogglingCheckedItem_ReturnsFalse()
    {
        var (db, list, itemId) = await SeedListWithItemAsync();
        var handler = BuildHandler(db: db);

        // Toggle once → checked
        await handler.Handle(
            new ToggleListItemCommand(list.Id.Value, itemId.Value, Guid.NewGuid()),
            CancellationToken.None);

        // Toggle again → unchecked
        var result = await handler.Handle(
            new ToggleListItemCommand(list.Id.Value, itemId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Checked.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ReturnsCorrectUncheckedCount()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = ListTestHelpers.MakeList(familyId);
        var id1 = ListItemId.New();
        var id2 = ListItemId.New();
        list.AddItem(id1, ListItemName.Create("A"), null, null, DateTime.UtcNow);
        list.AddItem(id2, ListItemName.Create("B"), null, null, DateTime.UtcNow);
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new ToggleListItemCommand(list.Id.Value, id1.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.UncheckedCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_TogglesItem_ReturnsItemId()
    {
        var (db, list, itemId) = await SeedListWithItemAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new ToggleListItemCommand(list.Id.Value, itemId.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.ItemId.Should().Be(itemId.Value);
    }

    [Fact]
    public async Task Handle_PersistsCheckedStateChange()
    {
        var (db, list, itemId) = await SeedListWithItemAsync();
        var handler = BuildHandler(db: db);

        await handler.Handle(
            new ToggleListItemCommand(list.Id.Value, itemId.Value, Guid.NewGuid()),
            CancellationToken.None);

        var savedList = await db.Set<SharedList>()
            .Include(l => l.Items)
            .SingleAsync(l => l.Id == list.Id);
        savedList.Items.Single().Checked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ListNotFound_ThrowsListException()
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new ToggleListItemCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.ListNotFound);
    }

    [Fact]
    public async Task Handle_ItemNotFound_ThrowsListException()
    {
        var (db, list, _) = await SeedListWithItemAsync();
        var handler = BuildHandler(db: db);
        var missingItemId = Guid.NewGuid();

        var act = () => handler.Handle(
            new ToggleListItemCommand(list.Id.Value, missingItemId, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.ItemNotFound);
    }

    [Fact]
    public async Task Handle_WhenAccessDenied_ThrowsListException()
    {
        var (db, list, itemId) = await SeedListWithItemAsync();
        var auth = new StubListAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db: db, auth: auth);

        var act = () => handler.Handle(
            new ToggleListItemCommand(list.Id.Value, itemId.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_WritesEventToLog()
    {
        var (db, list, itemId) = await SeedListWithItemAsync();
        var eventLog = new StubListEventLogWriter();
        var handler = new ToggleListItemCommandHandler(db, eventLog, new StubListAuthorizationService());

        await handler.Handle(
            new ToggleListItemCommand(list.Id.Value, itemId.Value, Guid.NewGuid()),
            CancellationToken.None);

        eventLog.WrittenEvents.Should().HaveCount(1);
    }
}
