using DomusMind.Application.Features.Lists;
using DomusMind.Application.Features.Lists.CreateLinkedListForEvent;
using DomusMind.Application.Tests.Features.Calendar;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Lists;

public sealed class CreateLinkedListForEventCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static CreateLinkedListForEventCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new CreateLinkedListForEventCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubListAuthorizationService());
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
            new CreateLinkedListForEventCommand(evt.Id.Value, familyId, null, Guid.NewGuid()),
            CancellationToken.None);

        result.Name.Should().Be("Birthday Party checklist");
        result.LinkedPlanId.Should().Be(evt.Id.Value);
    }

    [Fact]
    public async Task Handle_CreatesLinkedList_WithCustomName()
    {
        var (db, evt, familyId) = await SeedEventAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new CreateLinkedListForEventCommand(evt.Id.Value, familyId, "Things to bring", Guid.NewGuid()),
            CancellationToken.None);

        result.Name.Should().Be("Things to bring");
    }

    [Fact]
    public async Task Handle_PersistsListToDatabase()
    {
        var (db, evt, familyId) = await SeedEventAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new CreateLinkedListForEventCommand(evt.Id.Value, familyId, null, Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<SharedList>()
            .SingleOrDefaultAsync(l => l.Id == ListId.From(result.ListId));
        saved.Should().NotBeNull();
        saved!.LinkedEntityType.Should().Be("CalendarEvent");
        saved!.LinkedEntityId.Should().Be(evt.Id.Value);
    }

    [Fact]
    public async Task Handle_CalendarEventNotFound_ThrowsListException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db: db);

        var act = async () => await handler.Handle(
            new CreateLinkedListForEventCommand(Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_CalendarEventFromDifferentFamily_ThrowsListException()
    {
        var (db, evt, _) = await SeedEventAsync();
        var handler = BuildHandler(db: db);

        // Use a different familyId
        var act = async () => await handler.Handle(
            new CreateLinkedListForEventCommand(evt.Id.Value, Guid.NewGuid(), null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsListException()
    {
        var (db, evt, familyId) = await SeedEventAsync();
        var auth = new StubListAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db: db, auth: auth);

        var act = async () => await handler.Handle(
            new CreateLinkedListForEventCommand(evt.Id.Value, familyId, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.AccessDenied);
    }
}
