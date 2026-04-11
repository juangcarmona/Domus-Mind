using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Features.Calendar.GetMemberAgenda;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Calendar;

public sealed class GetMemberAgendaQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetMemberAgendaQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubCalendarAuthorizationService? auth = null)
        => new(db, auth ?? new StubCalendarAuthorizationService());

    private static SharedList MakeListWithItem(
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

    private static GetMemberAgendaQuery BuildQuery(
        Guid familyId,
        string? from = null,
        string? to = null)
        => new(familyId, Guid.NewGuid(), from, to, Guid.NewGuid());

    // ── Access control ────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_AccessDenied_ThrowsCalendarException()
    {
        var db = CreateDb();
        var auth = new StubCalendarAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            BuildQuery(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.AccessDenied);
    }

    // ── Due-date list item projection ──────────────────────────────────────

    [Fact]
    public async Task Handle_ItemWithDueDateInWindow_AppearsInAgenda()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = MakeListWithItem(familyId, "Groceries", "Milk", dueDate: new DateOnly(2026, 4, 11));
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            BuildQuery(familyId.Value, "2026-04-11", "2026-04-11"),
            CancellationToken.None);

        result.Items.Should().ContainSingle(i => i.Type == "list-item" && i.Title == "Milk");
    }

    [Fact]
    public async Task Handle_ItemWithDueDateOutsideWindow_DoesNotAppear()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = MakeListWithItem(familyId, "Groceries", "Milk", dueDate: new DateOnly(2026, 4, 20));
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            BuildQuery(familyId.Value, "2026-04-11", "2026-04-12"),
            CancellationToken.None);

        result.Items.Should().NotContain(i => i.Type == "list-item");
    }

    // ── Reminder-based projection ──────────────────────────────────────────

    [Fact]
    public async Task Handle_ItemWithReminderInWindow_AppearsInAgenda()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var reminderTime = new DateTimeOffset(2026, 4, 11, 9, 0, 0, TimeSpan.Zero);
        var list = MakeListWithItem(familyId, "Tasks", "Call dentist", reminder: reminderTime);
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            BuildQuery(familyId.Value, "2026-04-11", "2026-04-11"),
            CancellationToken.None);

        result.Items.Should().ContainSingle(i => i.Type == "list-item" && i.Title == "Call dentist");
    }

    // ── Type discrimination ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ListItemEntry_HasTypeListItem()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = MakeListWithItem(familyId, "Groceries", "Milk", dueDate: new DateOnly(2026, 4, 11));
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            BuildQuery(familyId.Value, "2026-04-11", "2026-04-11"),
            CancellationToken.None);

        var item = result.Items.Should().ContainSingle(i => i.Title == "Milk").Subject;
        item.Type.Should().Be("list-item");
        item.EventId.Should().BeNull();
        item.TaskId.Should().BeNull();
        item.RoutineId.Should().BeNull();
    }

    // ── Contract fields for list-item type ────────────────────────────────

    [Fact]
    public async Task Handle_ListItemEntry_IncludesListName()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = MakeListWithItem(familyId, "Weekly Groceries", "Eggs", dueDate: new DateOnly(2026, 4, 11));
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            BuildQuery(familyId.Value, "2026-04-11", "2026-04-11"),
            CancellationToken.None);

        var item = result.Items.Should().ContainSingle(i => i.Type == "list-item").Subject;
        item.ListName.Should().Be("Weekly Groceries");
    }

    [Fact]
    public async Task Handle_ListItemEntry_IncludesListIdAndItemId()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = MakeListWithItem(familyId, "Groceries", "Milk", dueDate: new DateOnly(2026, 4, 11));
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            BuildQuery(familyId.Value, "2026-04-11", "2026-04-11"),
            CancellationToken.None);

        var item = result.Items.Should().ContainSingle(i => i.Type == "list-item").Subject;
        item.ListId.Should().NotBeNull();
        item.ListItemId.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ImportantListItem_HasImportanceTrue()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = MakeListWithItem(familyId, "Groceries", "Urgent Milk", dueDate: new DateOnly(2026, 4, 11), importance: true);
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            BuildQuery(familyId.Value, "2026-04-11", "2026-04-11"),
            CancellationToken.None);

        var item = result.Items.Should().ContainSingle(i => i.Type == "list-item").Subject;
        item.Importance.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ListItemEntry_IncludesDueDate()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var dueDate = new DateOnly(2026, 4, 11);
        var list = MakeListWithItem(familyId, "Groceries", "Milk", dueDate: dueDate);
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            BuildQuery(familyId.Value, "2026-04-11", "2026-04-11"),
            CancellationToken.None);

        var item = result.Items.Should().ContainSingle(i => i.Type == "list-item").Subject;
        item.DueDate.Should().Be(dueDate);
    }

    [Fact]
    public async Task Handle_ListItemWithReminder_IncludesReminderField()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var reminder = new DateTimeOffset(2026, 4, 11, 9, 0, 0, TimeSpan.Zero);
        var list = MakeListWithItem(familyId, "Tasks", "Call dentist", reminder: reminder);
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            BuildQuery(familyId.Value, "2026-04-11", "2026-04-11"),
            CancellationToken.None);

        var item = result.Items.Should().ContainSingle(i => i.Type == "list-item").Subject;
        item.Reminder.Should().Be(reminder);
    }

    // ── Read-only flag ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ListItemEntry_IsReadOnly()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = MakeListWithItem(familyId, "Groceries", "Milk", dueDate: new DateOnly(2026, 4, 11));
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            BuildQuery(familyId.Value, "2026-04-11", "2026-04-11"),
            CancellationToken.None);

        var item = result.Items.Should().ContainSingle(i => i.Type == "list-item").Subject;
        item.IsReadOnly.Should().BeTrue();
    }

    // ── Household scoping ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_AllHouseholdTemporalItems_AppearRegardlessOfMember()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list1 = MakeListWithItem(familyId, "Groceries", "Milk", dueDate: new DateOnly(2026, 4, 11));
        var list2 = MakeListWithItem(familyId, "Chores", "Vacuum", dueDate: new DateOnly(2026, 4, 11));
        db.Set<SharedList>().AddRange(list1, list2);
        await db.SaveChangesAsync();

        // Query from the perspective of any member — both household items appear
        var result = await BuildHandler(db).Handle(
            BuildQuery(familyId.Value, "2026-04-11", "2026-04-11"),
            CancellationToken.None);

        result.Items.Count(i => i.Type == "list-item").Should().Be(2);
    }

    [Fact]
    public async Task Handle_ItemsFromOtherFamily_DoNotAppear()
    {
        var db = CreateDb();
        var familyA = FamilyId.New();
        var familyB = FamilyId.New();
        var listA = MakeListWithItem(familyA, "Groceries", "Family A Milk", dueDate: new DateOnly(2026, 4, 11));
        var listB = MakeListWithItem(familyB, "Groceries", "Family B Milk", dueDate: new DateOnly(2026, 4, 11));
        db.Set<SharedList>().AddRange(listA, listB);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).Handle(
            BuildQuery(familyA.Value, "2026-04-11", "2026-04-11"),
            CancellationToken.None);

        result.Items.Where(i => i.Type == "list-item").Should().ContainSingle()
            .Which.Title.Should().Be("Family A Milk");
    }
}
