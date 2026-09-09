using DomusMind.Application.Features.Lists;
using DomusMind.Application.Features.Lists.RenameList;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Lists;

public sealed class RenameListCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static RenameListCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new RenameListCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubListAuthorizationService());
    }

    private static async Task<(DomusMindDbContext db, SharedList list)> SeedListAsync()
    {
        var db = CreateDb();
        var list = ListTestHelpers.MakeList(FamilyId.New());
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();
        return (db, list);
    }

    [Fact]
    public async Task Handle_RenamesList_ReturnsNewName()
    {
        var (db, list) = await SeedListAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new RenameListCommand(list.Id.Value, "Renamed List", Guid.NewGuid()),
            CancellationToken.None);

        result.ListId.Should().Be(list.Id.Value);
        result.Name.Should().Be("Renamed List");
    }

    [Fact]
    public async Task Handle_PersistsNewName()
    {
        var (db, list) = await SeedListAsync();
        var handler = BuildHandler(db: db);

        await handler.Handle(
            new RenameListCommand(list.Id.Value, "Persisted Name", Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<SharedList>().SingleAsync(l => l.Id == list.Id);
        saved.Name.Value.Should().Be("Persisted Name");
    }

    [Fact]
    public async Task Handle_EmptyName_ThrowsListException()
    {
        var (db, list) = await SeedListAsync();
        var handler = BuildHandler(db: db);

        var act = async () => await handler.Handle(
            new RenameListCommand(list.Id.Value, "   ", Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_ListNotFound_ThrowsListException()
    {
        var handler = BuildHandler();

        var act = async () => await handler.Handle(
            new RenameListCommand(Guid.NewGuid(), "Name", Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.ListNotFound);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsListException()
    {
        var (db, list) = await SeedListAsync();
        var auth = new StubListAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db: db, auth: auth);

        var act = async () => await handler.Handle(
            new RenameListCommand(list.Id.Value, "New Name", Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.AccessDenied);
    }
}
