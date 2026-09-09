using DomusMind.Application.Features.Lists;
using DomusMind.Application.Features.Lists.GetListByLinkedEntity;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Lists;

public sealed class GetListByLinkedEntityQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetListByLinkedEntityQueryHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new GetListByLinkedEntityQueryHandler(
            context, auth ?? new StubListAuthorizationService());
    }

    private static async Task<(DomusMindDbContext db, SharedList list, Guid entityId)> SeedLinkedListAsync()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = ListTestHelpers.MakeList(familyId, "Event Checklist");
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
            new GetListByLinkedEntityQuery("CalendarEvent", entityId, Guid.NewGuid()),
            CancellationToken.None);

        result.ListId.Should().Be(list.Id.Value);
        result.Name.Should().Be("Event Checklist");
    }

    [Fact]
    public async Task Handle_ReturnsCorrectItemCounts()
    {
        var (db, list, entityId) = await SeedLinkedListAsync();

        // Add items directly to make count non-zero
        list.AddItem(ListItemId.New(), ListItemName.Create("Item 1"), null, null, DateTime.UtcNow);
        list.AddItem(ListItemId.New(), ListItemName.Create("Item 2"), null, null, DateTime.UtcNow);
        list.ClearDomainEvents();
        await db.SaveChangesAsync();

        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new GetListByLinkedEntityQuery("CalendarEvent", entityId, Guid.NewGuid()),
            CancellationToken.None);

        result.ItemCount.Should().Be(2);
        result.UncheckedCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_NoLinkedList_ThrowsListException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db: db);

        var act = async () => await handler.Handle(
            new GetListByLinkedEntityQuery("CalendarEvent", Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.ListNotFound);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsListException()
    {
        var (db, _, entityId) = await SeedLinkedListAsync();
        var auth = new StubListAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db: db, auth: auth);

        var act = async () => await handler.Handle(
            new GetListByLinkedEntityQuery("CalendarEvent", entityId, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.AccessDenied);
    }
}
