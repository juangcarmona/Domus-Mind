using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Features.Calendar.GetFamilyTimeline;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Calendar;

public sealed class GetFamilyTimelineQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetFamilyTimelineQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubCalendarAuthorizationService? auth = null)
        => new(db, auth ?? new StubCalendarAuthorizationService());

    private static SharedList MakeListWithTemporalItem(
        FamilyId familyId,
        string listName,
        string itemName,
        DateOnly? dueDate = null,
        DateTimeOffset? reminder = null,
        bool importance = false)
    {
        var list = SharedList.Create(
            ListId.New(),
            familyId,
            ListName.Create(listName),
            ListKind.Create("Shopping"),
            areaId: null,
            linkedEntityType: null,
            linkedEntityId: null,
            createdAtUtc: DateTime.UtcNow);

        var item = list.AddItem(ListItemId.New(), ListItemName.Create(itemName), null, null, DateTime.UtcNow);
        list.SetItemImportance(item.Id, importance, DateTime.UtcNow);

        if (dueDate.HasValue || reminder.HasValue)
            list.SetItemTemporal(item.Id, dueDate, reminder, null, DateTime.UtcNow);

        return list;
    }

    [Fact]
    public async Task Handle_WithFamilyEvents_ReturnsOrderedByDate()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var later = CalendarTestHelpers.MakeEvent(familyId, "Later Event", new DateOnly(2026, 4, 10));
        var sooner = CalendarTestHelpers.MakeEvent(familyId, "Sooner Event", new DateOnly(2026, 4, 1));
        db.Set<Domain.Calendar.CalendarEvent>().AddRange(later, sooner);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetFamilyTimelineQuery(familyId.Value, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.Events.Should().HaveCount(2);
        result.Events.First().Title.Should().Be("Sooner Event");
        result.Events.Last().Title.Should().Be("Later Event");
    }

    [Fact]
    public async Task Handle_NoEvents_ReturnsEmptyList()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetFamilyTimelineQuery(Guid.NewGuid(), null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.Events.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsCalendarException()
    {
        var db = CreateDb();
        var auth = new StubCalendarAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new GetFamilyTimelineQuery(Guid.NewGuid(), null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_FiltersByFromDate()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var past = CalendarTestHelpers.MakeEvent(familyId, "Past Event", new DateOnly(2026, 3, 1));
        var future = CalendarTestHelpers.MakeEvent(familyId, "Future Event", new DateOnly(2026, 4, 10));
        db.Set<Domain.Calendar.CalendarEvent>().AddRange(past, future);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetFamilyTimelineQuery(familyId.Value, new DateOnly(2026, 4, 1), null, Guid.NewGuid()),
            CancellationToken.None);

        result.Events.Should().ContainSingle()
            .Which.Title.Should().Be("Future Event");
    }

    [Fact]
    public async Task Handle_ExcludesOtherFamilyEvents()
    {
        var db = CreateDb();
        var familyA = FamilyId.New();
        var familyB = FamilyId.New();
        var eventA = CalendarTestHelpers.MakeEvent(familyA, "Family A Event", new DateOnly(2026, 4, 1));
        var eventB = CalendarTestHelpers.MakeEvent(familyB, "Family B Event", new DateOnly(2026, 4, 1));
        db.Set<Domain.Calendar.CalendarEvent>().AddRange(eventA, eventB);
        await db.SaveChangesAsync();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetFamilyTimelineQuery(familyA.Value, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.Events.Should().ContainSingle()
            .Which.Title.Should().Be("Family A Event");
    }

    // ── Temporal list item projection ──────────────────────────────────────

    [Fact]
    public async Task Handle_ItemWithDueDateInWindow_AppearsInListItems()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = MakeListWithTemporalItem(familyId, "Groceries", "Milk", dueDate: new DateOnly(2026, 4, 10));
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            new GetFamilyTimelineQuery(familyId.Value, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30), Guid.NewGuid()),
            CancellationToken.None);

        result.ListItems.Should().ContainSingle(i => i.ItemName == "Milk");
    }

    [Fact]
    public async Task Handle_ItemWithDueDateOutsideWindow_DoesNotAppear()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = MakeListWithTemporalItem(familyId, "Groceries", "Milk", dueDate: new DateOnly(2026, 5, 15));
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            new GetFamilyTimelineQuery(familyId.Value, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30), Guid.NewGuid()),
            CancellationToken.None);

        result.ListItems.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ItemWithReminderInWindow_AppearsInListItems()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var reminder = new DateTimeOffset(2026, 4, 15, 9, 0, 0, TimeSpan.Zero);
        var list = MakeListWithTemporalItem(familyId, "Tasks", "Call doctor", reminder: reminder);
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            new GetFamilyTimelineQuery(familyId.Value, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30), Guid.NewGuid()),
            CancellationToken.None);

        result.ListItems.Should().ContainSingle(i => i.ItemName == "Call doctor");
    }

    [Fact]
    public async Task Handle_ItemWithNoTemporalFields_DoesNotAppear()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        // item with no temporal fields — not Agenda-eligible
        var list = SharedList.Create(
            ListId.New(), familyId,
            ListName.Create("Notes"), ListKind.Create("Shopping"),
            null, null, null, DateTime.UtcNow);
        list.AddItem(ListItemId.New(), ListItemName.Create("No date item"), null, null, DateTime.UtcNow);
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            new GetFamilyTimelineQuery(familyId.Value, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.ListItems.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ListItems_IncludeCorrectListMetadata()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = MakeListWithTemporalItem(familyId, "Shopping List", "Butter", dueDate: new DateOnly(2026, 4, 10));
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            new GetFamilyTimelineQuery(familyId.Value, null, null, Guid.NewGuid()),
            CancellationToken.None);

        var item = result.ListItems.Should().ContainSingle().Subject;
        item.ListName.Should().Be("Shopping List");
        item.ItemName.Should().Be("Butter");
        item.ListId.Should().NotBe(Guid.Empty);
        item.ItemId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ImportantListItem_HasImportanceTrue()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = MakeListWithTemporalItem(familyId, "Groceries", "Urgent item", dueDate: new DateOnly(2026, 4, 10), importance: true);
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            new GetFamilyTimelineQuery(familyId.Value, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.ListItems.Should().ContainSingle().Which.Importance.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NonImportantListItem_HasImportanceFalse()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = MakeListWithTemporalItem(familyId, "Groceries", "Normal item", dueDate: new DateOnly(2026, 4, 10), importance: false);
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            new GetFamilyTimelineQuery(familyId.Value, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.ListItems.Should().ContainSingle().Which.Importance.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ListItemsFromOtherFamily_DoNotAppear()
    {
        var db = CreateDb();
        var familyA = FamilyId.New();
        var familyB = FamilyId.New();
        var listA = MakeListWithTemporalItem(familyA, "List A", "Item A", dueDate: new DateOnly(2026, 4, 10));
        var listB = MakeListWithTemporalItem(familyB, "List B", "Item B", dueDate: new DateOnly(2026, 4, 10));
        db.Set<SharedList>().AddRange(listA, listB);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            new GetFamilyTimelineQuery(familyA.Value, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.ListItems.Should().ContainSingle().Which.ItemName.Should().Be("Item A");
    }
}

