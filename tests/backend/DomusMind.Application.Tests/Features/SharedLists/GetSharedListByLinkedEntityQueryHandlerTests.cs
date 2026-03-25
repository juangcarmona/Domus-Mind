using DomusMind.Application.Features.SharedLists;
using DomusMind.Application.Features.SharedLists.GetSharedListByLinkedEntity;
using DomusMind.Domain.Family;
using DomusMind.Domain.SharedLists;
using DomusMind.Domain.SharedLists.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.SharedLists;

public sealed class GetSharedListByLinkedEntityQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetSharedListByLinkedEntityQueryHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubSharedListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new GetSharedListByLinkedEntityQueryHandler(
            context, auth ?? new StubSharedListAuthorizationService());
    }

    private static async Task<(DomusMindDbContext db, SharedList list, Guid entityId)> SeedLinkedListAsync()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = SharedListTestHelpers.MakeList(familyId, "Event Checklist");
        var entityId = Guid.NewGuid();
        list.LinkToEntity("CalendarEvent", entityId, DateTime.UtcNow);
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();
        return (db, list, entityId);
    }

    [Fact]
    public async Task Handle_ReturnsLinkedList()
    {
        var (db, list, entityId) = await SeedLinkedListAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetSharedListByLinkedEntityQuery("CalendarEvent", entityId, Guid.NewGuid()),
            CancellationToken.None);

        result.ListId.Should().Be(list.Id.Value);
        result.Name.Should().Be("Event Checklist");
    }

    [Fact]
    public async Task Handle_ReturnsCorrectItemCounts()
    {
        var (db, list, entityId) = await SeedLinkedListAsync();

        // Add items directly to make count non-zero
        list.AddItem(SharedListItemId.New(), SharedListItemName.Create("Item 1"), null, null, DateTime.UtcNow);
        list.AddItem(SharedListItemId.New(), SharedListItemName.Create("Item 2"), null, null, DateTime.UtcNow);
        list.ClearDomainEvents();
        await db.SaveChangesAsync();

        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetSharedListByLinkedEntityQuery("CalendarEvent", entityId, Guid.NewGuid()),
            CancellationToken.None);

        result.ItemCount.Should().Be(2);
        result.UncheckedCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_NoLinkedList_ThrowsSharedListException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db: db);

        var act = async () => await handler.Handle(
            new GetSharedListByLinkedEntityQuery("CalendarEvent", Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(e => e.Code == SharedListErrorCode.ListNotFound);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsSharedListException()
    {
        var (db, _, entityId) = await SeedLinkedListAsync();
        var auth = new StubSharedListAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db: db, auth: auth);

        var act = async () => await handler.Handle(
            new GetSharedListByLinkedEntityQuery("CalendarEvent", entityId, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(e => e.Code == SharedListErrorCode.AccessDenied);
    }
}
