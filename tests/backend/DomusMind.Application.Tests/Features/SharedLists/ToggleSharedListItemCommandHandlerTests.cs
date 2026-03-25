using DomusMind.Application.Features.SharedLists;
using DomusMind.Application.Features.SharedLists.ToggleSharedListItem;
using DomusMind.Domain.Family;
using DomusMind.Domain.SharedLists;
using DomusMind.Domain.SharedLists.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.SharedLists;

public sealed class ToggleSharedListItemCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ToggleSharedListItemCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubSharedListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new ToggleSharedListItemCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubSharedListAuthorizationService());
    }

    private static async Task<(DomusMindDbContext db, SharedList list, SharedListItemId itemId)> SeedListWithItemAsync()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = SharedListTestHelpers.MakeList(familyId);
        var itemId = SharedListItemId.New();
        list.AddItem(itemId, SharedListItemName.Create("Bread"), null, null, DateTime.UtcNow);
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
            new ToggleSharedListItemCommand(list.Id.Value, itemId.Value, null, Guid.NewGuid()),
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
            new ToggleSharedListItemCommand(list.Id.Value, itemId.Value, null, Guid.NewGuid()),
            CancellationToken.None);

        // Toggle again → unchecked
        var result = await handler.Handle(
            new ToggleSharedListItemCommand(list.Id.Value, itemId.Value, null, Guid.NewGuid()),
            CancellationToken.None);

        result.Checked.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ReturnsCorrectUncheckedCount()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = SharedListTestHelpers.MakeList(familyId);
        var id1 = SharedListItemId.New();
        var id2 = SharedListItemId.New();
        list.AddItem(id1, SharedListItemName.Create("A"), null, null, DateTime.UtcNow);
        list.AddItem(id2, SharedListItemName.Create("B"), null, null, DateTime.UtcNow);
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new ToggleSharedListItemCommand(list.Id.Value, id1.Value, null, Guid.NewGuid()),
            CancellationToken.None);

        result.UncheckedCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithMemberId_ReturnsUpdatedByMemberId()
    {
        var (db, list, itemId) = await SeedListWithItemAsync();
        var handler = BuildHandler(db: db);
        var memberId = Guid.NewGuid();

        var result = await handler.Handle(
            new ToggleSharedListItemCommand(list.Id.Value, itemId.Value, memberId, Guid.NewGuid()),
            CancellationToken.None);

        result.UpdatedByMemberId.Should().Be(memberId);
    }

    [Fact]
    public async Task Handle_PersistsCheckedStateChange()
    {
        var (db, list, itemId) = await SeedListWithItemAsync();
        var handler = BuildHandler(db: db);

        await handler.Handle(
            new ToggleSharedListItemCommand(list.Id.Value, itemId.Value, null, Guid.NewGuid()),
            CancellationToken.None);

        var savedList = await db.Set<SharedList>()
            .Include(l => l.Items)
            .SingleAsync(l => l.Id == list.Id);
        savedList.Items.Single().Checked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ListNotFound_ThrowsSharedListException()
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new ToggleSharedListItemCommand(Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(e => e.Code == SharedListErrorCode.ListNotFound);
    }

    [Fact]
    public async Task Handle_ItemNotFound_ThrowsSharedListException()
    {
        var (db, list, _) = await SeedListWithItemAsync();
        var handler = BuildHandler(db: db);
        var missingItemId = Guid.NewGuid();

        var act = () => handler.Handle(
            new ToggleSharedListItemCommand(list.Id.Value, missingItemId, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(e => e.Code == SharedListErrorCode.ItemNotFound);
    }

    [Fact]
    public async Task Handle_WhenAccessDenied_ThrowsSharedListException()
    {
        var (db, list, itemId) = await SeedListWithItemAsync();
        var auth = new StubSharedListAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db: db, auth: auth);

        var act = () => handler.Handle(
            new ToggleSharedListItemCommand(list.Id.Value, itemId.Value, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(e => e.Code == SharedListErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_WritesEventToLog()
    {
        var (db, list, itemId) = await SeedListWithItemAsync();
        var eventLog = new StubSharedListEventLogWriter();
        var handler = new ToggleSharedListItemCommandHandler(db, eventLog, new StubSharedListAuthorizationService());

        await handler.Handle(
            new ToggleSharedListItemCommand(list.Id.Value, itemId.Value, null, Guid.NewGuid()),
            CancellationToken.None);

        eventLog.WrittenEvents.Should().HaveCount(1);
    }
}
