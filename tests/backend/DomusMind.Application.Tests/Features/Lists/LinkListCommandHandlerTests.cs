using DomusMind.Application.Features.Lists;
using DomusMind.Application.Features.Lists.LinkList;
using DomusMind.Domain.Lists;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Lists;

public sealed class LinkListCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static LinkListCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new LinkListCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubListAuthorizationService());
    }

    private static async Task<(DomusMindDbContext db, SharedList list)> SeedListAsync()
    {
        var db = CreateDb();
        var familyId = Domain.Family.FamilyId.New();
        var list = ListTestHelpers.MakeList(familyId);
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();
        return (db, list);
    }

    [Fact]
    public async Task Handle_LinksListToCalendarEvent()
    {
        var (db, list) = await SeedListAsync();
        var handler = BuildHandler(db: db);
        var eventId = Guid.NewGuid();

        var result = await handler.Handle(
            new LinkListCommand(list.Id.Value, "CalendarEvent", eventId, Guid.NewGuid()),
            CancellationToken.None);

        result.ListId.Should().Be(list.Id.Value);
        result.LinkedEntityType.Should().Be("CalendarEvent");
        result.LinkedEntityId.Should().Be(eventId);
    }

    [Fact]
    public async Task Handle_PersistsLinkage()
    {
        var (db, list) = await SeedListAsync();
        var handler = BuildHandler(db: db);
        var eventId = Guid.NewGuid();

        await handler.Handle(
            new LinkListCommand(list.Id.Value, "CalendarEvent", eventId, Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<SharedList>()
            .SingleAsync(l => l.Id == list.Id);
        saved.LinkedEntityType.Should().Be("CalendarEvent");
        saved.LinkedEntityId.Should().Be(eventId);
    }

    [Fact]
    public async Task Handle_UnsupportedEntityType_ThrowsListException()
    {
        var (db, list) = await SeedListAsync();
        var handler = BuildHandler(db: db);

        var act = async () => await handler.Handle(
            new LinkListCommand(list.Id.Value, "Task", Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_ListNotFound_ThrowsListException()
    {
        var handler = BuildHandler();

        var act = async () => await handler.Handle(
            new LinkListCommand(Guid.NewGuid(), "CalendarEvent", Guid.NewGuid(), Guid.NewGuid()),
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
            new LinkListCommand(list.Id.Value, "CalendarEvent", Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.AccessDenied);
    }
}
