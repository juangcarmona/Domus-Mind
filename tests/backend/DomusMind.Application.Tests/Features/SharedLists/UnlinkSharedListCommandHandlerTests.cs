using DomusMind.Application.Features.SharedLists;
using DomusMind.Application.Features.SharedLists.UnlinkSharedList;
using DomusMind.Domain.SharedLists;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.SharedLists;

public sealed class UnlinkSharedListCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static UnlinkSharedListCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubSharedListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new UnlinkSharedListCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubSharedListAuthorizationService());
    }

    private static async Task<(DomusMindDbContext db, SharedList list)> SeedLinkedListAsync()
    {
        var db = CreateDb();
        var familyId = Domain.Family.FamilyId.New();
        var list = SharedListTestHelpers.MakeList(familyId);
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
            new UnlinkSharedListCommand(list.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_PersistsClearedLinkage()
    {
        var (db, list) = await SeedLinkedListAsync();
        var handler = BuildHandler(db: db);

        await handler.Handle(
            new UnlinkSharedListCommand(list.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<SharedList>()
            .SingleAsync(l => l.Id == list.Id);
        saved.LinkedEntityType.Should().BeNull();
        saved.LinkedEntityId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ListNotFound_ThrowsSharedListException()
    {
        var handler = BuildHandler();

        var act = async () => await handler.Handle(
            new UnlinkSharedListCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(e => e.Code == SharedListErrorCode.ListNotFound);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsSharedListException()
    {
        var (db, list) = await SeedLinkedListAsync();
        var auth = new StubSharedListAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db: db, auth: auth);

        var act = async () => await handler.Handle(
            new UnlinkSharedListCommand(list.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(e => e.Code == SharedListErrorCode.AccessDenied);
    }
}
