using DomusMind.Application.Features.Lists;
using DomusMind.Application.Features.Lists.UnlinkList;
using DomusMind.Domain.Lists;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Lists;

public sealed class UnlinkListCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static UnlinkListCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new UnlinkListCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubListAuthorizationService());
    }

    private static async Task<(DomusMindDbContext db, SharedList list)> SeedLinkedListAsync()
    {
        var db = CreateDb();
        var familyId = Domain.Family.FamilyId.New();
        var list = ListTestHelpers.MakeList(familyId);
        list.LinkToEntity("CalendarEvent", Guid.NewGuid(), DateTime.UtcNow);
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();
        return (db, list);
    }

    [Fact]
    public async Task Handle_UnlinksEntityFromList()
    {
        var (db, list) = await SeedLinkedListAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new UnlinkListCommand(list.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_PersistsClearedLinkage()
    {
        var (db, list) = await SeedLinkedListAsync();
        var handler = BuildHandler(db: db);

        await handler.Handle(
            new UnlinkListCommand(list.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<SharedList>()
            .SingleAsync(l => l.Id == list.Id);
        saved.LinkedEntityType.Should().BeNull();
        saved.LinkedEntityId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ListNotFound_ThrowsListException()
    {
        var handler = BuildHandler();

        var act = async () => await handler.Handle(
            new UnlinkListCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.ListNotFound);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsListException()
    {
        var (db, list) = await SeedLinkedListAsync();
        var auth = new StubListAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db: db, auth: auth);

        var act = async () => await handler.Handle(
            new UnlinkListCommand(list.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.AccessDenied);
    }
}
