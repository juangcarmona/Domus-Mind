using DomusMind.Application.Features.SharedLists;
using DomusMind.Application.Features.SharedLists.CreateLinkedSharedListForEvent;
using DomusMind.Application.Tests.Features.Calendar;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Family;
using DomusMind.Domain.SharedLists;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.SharedLists;

public sealed class CreateLinkedSharedListForEventCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static CreateLinkedSharedListForEventCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubSharedListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new CreateLinkedSharedListForEventCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubSharedListAuthorizationService());
    }

    private static async Task<(DomusMindDbContext db, CalendarEvent calendarEvent, Guid familyId)> SeedEventAsync()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var calendarEvent = CalendarTestHelpers.MakeEvent(familyId, "Birthday Party", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)));
        db.Set<CalendarEvent>().Add(calendarEvent);
        await db.SaveChangesAsync();
        calendarEvent.ClearDomainEvents();
        return (db, calendarEvent, familyId.Value);
    }

    [Fact]
    public async Task Handle_CreatesLinkedList_WithDefaultName()
    {
        var (db, evt, familyId) = await SeedEventAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new CreateLinkedSharedListForEventCommand(evt.Id.Value, familyId, null, Guid.NewGuid()),
            CancellationToken.None);

        result.Name.Should().Be("Birthday Party checklist");
        result.LinkedEntityType.Should().Be("CalendarEvent");
        result.LinkedEntityId.Should().Be(evt.Id.Value);
    }

    [Fact]
    public async Task Handle_CreatesLinkedList_WithCustomName()
    {
        var (db, evt, familyId) = await SeedEventAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new CreateLinkedSharedListForEventCommand(evt.Id.Value, familyId, "Things to bring", Guid.NewGuid()),
            CancellationToken.None);

        result.Name.Should().Be("Things to bring");
    }

    [Fact]
    public async Task Handle_PersistsListToDatabase()
    {
        var (db, evt, familyId) = await SeedEventAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new CreateLinkedSharedListForEventCommand(evt.Id.Value, familyId, null, Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<SharedList>()
            .SingleOrDefaultAsync(l => l.Id == SharedListId.From(result.ListId));
        saved.Should().NotBeNull();
        saved!.LinkedEntityType.Should().Be("CalendarEvent");
        saved.LinkedEntityId.Should().Be(evt.Id.Value);
    }

    [Fact]
    public async Task Handle_CalendarEventNotFound_ThrowsSharedListException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db: db);

        var act = async () => await handler.Handle(
            new CreateLinkedSharedListForEventCommand(Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(e => e.Code == SharedListErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_CalendarEventFromDifferentFamily_ThrowsSharedListException()
    {
        var (db, evt, _) = await SeedEventAsync();
        var handler = BuildHandler(db: db);

        // Use a different familyId
        var act = async () => await handler.Handle(
            new CreateLinkedSharedListForEventCommand(evt.Id.Value, Guid.NewGuid(), null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(e => e.Code == SharedListErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsSharedListException()
    {
        var (db, evt, familyId) = await SeedEventAsync();
        var auth = new StubSharedListAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db: db, auth: auth);

        var act = async () => await handler.Handle(
            new CreateLinkedSharedListForEventCommand(evt.Id.Value, familyId, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(e => e.Code == SharedListErrorCode.AccessDenied);
    }
}
