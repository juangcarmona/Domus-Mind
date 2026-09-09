using DomusMind.Domain.Family;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.Events;
using DomusMind.Domain.Lists.ValueObjects;
using FluentAssertions;

namespace DomusMind.Domain.Tests.Lists;

public sealed class ListTests
{
    private static SharedList BuildList(
        string name = "Weekly Shopping",
        string kind = "Shopping",
        FamilyId? familyId = null)
    {
        return SharedList.Create(
            ListId.New(),
            familyId ?? FamilyId.New(),
            ListName.Create(name),
            ListKind.Create(kind),
            areaId: null,
            linkedEntityType: null,
            linkedEntityId: null,
            createdAtUtc: DateTime.UtcNow);
    }

    private static (ListItemId, ListItemName) NewItem(string name = "Milk")
        => (ListItemId.New(), ListItemName.Create(name));

    // ── Create ─────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidInputs_SetsName()
    {
        var list = BuildList("Weekly Shopping");

        list.Name.Value.Should().Be("Weekly Shopping");
    }

    [Fact]
    public void Create_WithValidInputs_SetsKind()
    {
        var list = BuildList(kind: "Shopping");

        list.Kind.Value.Should().Be("Shopping");
    }

    [Fact]
    public void Create_WithValidInputs_SetsFamilyId()
    {
        var familyId = FamilyId.New();
        var list = BuildList(familyId: familyId);

        list.FamilyId.Should().Be(familyId);
    }

    [Fact]
    public void Create_StartsWithNoItems()
    {
        var list = BuildList();

        list.Items.Should().BeEmpty();
    }

    [Fact]
    public void Create_EmitsListCreatedEvent()
    {
        var list = BuildList();

        list.DomainEvents.Should().HaveCount(1);
        list.DomainEvents.Single().Should().BeOfType<ListCreated>();
    }

    [Fact]
    public void Create_ListCreatedEvent_HasCorrectFamilyId()
    {
        var familyId = FamilyId.New();
        var list = BuildList(familyId: familyId);

        var evt = (ListCreated)list.DomainEvents.Single();
        evt.FamilyId.Should().Be(familyId.Value);
    }

    [Fact]
    public void Create_WithAreaId_SetsAreaId()
    {
        var areaId = ResponsibilityDomainId.New();
        var list = SharedList.Create(
            ListId.New(), FamilyId.New(),
            ListName.Create("Chores"), ListKind.Create("General"),
            areaId, null, null, DateTime.UtcNow);

        list.AreaId.Should().Be(areaId);
    }

    // ── ListName validation ───────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ListName_Create_WithEmptyValue_Throws(string name)
    {
        var act = () => ListName.Create(name);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ListName_Create_ExceedingMaxLength_Throws()
    {
        var tooLong = new string('x', ListName.MaxLength + 1);

        var act = () => ListName.Create(tooLong);

        act.Should().Throw<ArgumentException>();
    }

    // ── ListItemName validation ───────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ListItemName_Create_WithEmptyValue_Throws(string name)
    {
        var act = () => ListItemName.Create(name);

        act.Should().Throw<ArgumentException>();
    }

    // ── AddItem ─────────────────────────────────────────────────────────────

    [Fact]
    public void AddItem_AppendsUncheckedItem()
    {
        var list = BuildList();
        var (id, name) = NewItem("Eggs");

        list.AddItem(id, name, null, null, DateTime.UtcNow);

        list.Items.Should().HaveCount(1);
        list.Items.Single().Checked.Should().BeFalse();
    }

    [Fact]
    public void AddItem_AssignsOrderStartingAtOne()
    {
        var list = BuildList();
        var (id, name) = NewItem("Bread");

        list.AddItem(id, name, null, null, DateTime.UtcNow);

        list.Items.Single().Order.Should().Be(1);
    }

    [Fact]
    public void AddItem_MultipleItems_AssignsSequentialOrder()
    {
        var list = BuildList();

        list.AddItem(ListItemId.New(), ListItemName.Create("Apples"), null, null, DateTime.UtcNow);
        list.AddItem(ListItemId.New(), ListItemName.Create("Bananas"), null, null, DateTime.UtcNow);
        list.AddItem(ListItemId.New(), ListItemName.Create("Cherries"), null, null, DateTime.UtcNow);

        list.Items.Select(i => i.Order).Should().BeEquivalentTo([1, 2, 3]);
    }

    [Fact]
    public void AddItem_EmitsListItemAddedEvent()
    {
        var list = BuildList();
        list.ClearDomainEvents();
        var (id, name) = NewItem("Milk");

        list.AddItem(id, name, null, null, DateTime.UtcNow);

        list.DomainEvents.Should().HaveCount(1);
        list.DomainEvents.Single().Should().BeOfType<ListItemAdded>();
    }

    [Fact]
    public void AddItem_Event_HasCorrectItemName()
    {
        var list = BuildList();
        list.ClearDomainEvents();
        var (id, name) = NewItem("Cheese");

        list.AddItem(id, name, null, null, DateTime.UtcNow);

        var evt = (ListItemAdded)list.DomainEvents.Single();
        evt.ItemName.Should().Be("Cheese");
    }

    [Fact]
    public void AddItem_SetsQuantityAndNote()
    {
        var list = BuildList();
        var (id, name) = NewItem("Milk");

        var item = list.AddItem(id, name, "2 liters", "whole milk", DateTime.UtcNow);

        item.Quantity.Should().Be("2 liters");
        item.Note.Should().Be("whole milk");
    }

    [Fact]
    public void AddItem_ReturnsAddedItem()
    {
        var list = BuildList();
        var (id, name) = NewItem("Eggs");

        var item = list.AddItem(id, name, null, null, DateTime.UtcNow);

        item.Id.Should().Be(id);
        item.Name.Value.Should().Be("Eggs");
    }

    // ── ToggleItem ──────────────────────────────────────────────────────────

    [Fact]
    public void ToggleItem_UncheckedItem_BecomesChecked()
    {
        var list = BuildList();
        var (id, name) = NewItem("Butter");
        list.AddItem(id, name, null, null, DateTime.UtcNow);
        list.ClearDomainEvents();

        list.ToggleItem(id, null, DateTime.UtcNow);

        list.Items.Single().Checked.Should().BeTrue();
    }

    [Fact]
    public void ToggleItem_CheckedItem_BecomesUnchecked()
    {
        var list = BuildList();
        var (id, name) = NewItem("Butter");
        list.AddItem(id, name, null, null, DateTime.UtcNow);
        list.ToggleItem(id, null, DateTime.UtcNow);
        list.ClearDomainEvents();

        list.ToggleItem(id, null, DateTime.UtcNow);

        list.Items.Single().Checked.Should().BeFalse();
    }

    [Fact]
    public void ToggleItem_EmitsListItemToggledEvent()
    {
        var list = BuildList();
        var (id, name) = NewItem("Yogurt");
        list.AddItem(id, name, null, null, DateTime.UtcNow);
        list.ClearDomainEvents();

        list.ToggleItem(id, null, DateTime.UtcNow);

        list.DomainEvents.Should().HaveCount(1);
        list.DomainEvents.Single().Should().BeOfType<ListItemToggled>();
    }

    [Fact]
    public void ToggleItem_Event_ReflectsNewCheckedState()
    {
        var list = BuildList();
        var (id, name) = NewItem("Cream");
        list.AddItem(id, name, null, null, DateTime.UtcNow);
        list.ClearDomainEvents();

        list.ToggleItem(id, null, DateTime.UtcNow);

        var evt = (ListItemToggled)list.DomainEvents.Single();
        evt.Checked.Should().BeTrue();
    }

    [Fact]
    public void ToggleItem_WithUpdatedByMemberId_SetsMetadata()
    {
        var list = BuildList();
        var (id, name) = NewItem("Flour");
        list.AddItem(id, name, null, null, DateTime.UtcNow);
        var memberId = MemberId.New();

        var item = list.ToggleItem(id, memberId, DateTime.UtcNow);

        item.UpdatedByMemberId.Should().Be(memberId);
    }

    [Fact]
    public void ToggleItem_MissingItem_Throws()
    {
        var list = BuildList();
        var missingId = ListItemId.New();

        var act = () => list.ToggleItem(missingId, null, DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    // ── UncheckedCount ──────────────────────────────────────────────────────

    [Fact]
    public void UncheckedCount_ReturnsNumberOfUncheckedItems()
    {
        var list = BuildList();
        var id1 = ListItemId.New();
        var id2 = ListItemId.New();
        var id3 = ListItemId.New();
        list.AddItem(id1, ListItemName.Create("A"), null, null, DateTime.UtcNow);
        list.AddItem(id2, ListItemName.Create("B"), null, null, DateTime.UtcNow);
        list.AddItem(id3, ListItemName.Create("C"), null, null, DateTime.UtcNow);

        list.ToggleItem(id1, null, DateTime.UtcNow);

        list.UncheckedCount.Should().Be(2);
    }

    [Fact]
    public void UncheckedCount_EmptyList_ReturnsZero()
    {
        var list = BuildList();

        list.UncheckedCount.Should().Be(0);
    }

    // ── UpdateItem ──────────────────────────────────────────────────────────

    [Fact]
    public void UpdateItem_ChangesName()
    {
        var list = BuildList();
        var (id, name) = NewItem("Old Name");
        list.AddItem(id, name, null, null, DateTime.UtcNow);
        list.ClearDomainEvents();

        list.UpdateItem(id, ListItemName.Create("New Name"), null, null, DateTime.UtcNow);

        list.Items.Single().Name.Value.Should().Be("New Name");
    }

    [Fact]
    public void UpdateItem_ChangesQuantityAndNote()
    {
        var list = BuildList();
        var (id, name) = NewItem("Milk");
        list.AddItem(id, name, null, null, DateTime.UtcNow);

        list.UpdateItem(id, name, "2L", "skimmed", DateTime.UtcNow);

        var item = list.Items.Single();
        item.Quantity.Should().Be("2L");
        item.Note.Should().Be("skimmed");
    }

    [Fact]
    public void UpdateItem_EmitsListItemUpdatedEvent()
    {
        var list = BuildList();
        var (id, name) = NewItem("Eggs");
        list.AddItem(id, name, null, null, DateTime.UtcNow);
        list.ClearDomainEvents();

        list.UpdateItem(id, ListItemName.Create("Organic Eggs"), null, null, DateTime.UtcNow);

        list.DomainEvents.Should().HaveCount(1);
        list.DomainEvents.Single().Should().BeOfType<ListItemUpdated>();
    }

    [Fact]
    public void UpdateItem_Event_HasCorrectNewName()
    {
        var list = BuildList();
        var (id, name) = NewItem("Bread");
        list.AddItem(id, name, null, null, DateTime.UtcNow);
        list.ClearDomainEvents();

        list.UpdateItem(id, ListItemName.Create("Sourdough Bread"), null, null, DateTime.UtcNow);

        var evt = (ListItemUpdated)list.DomainEvents.Single();
        evt.NewName.Should().Be("Sourdough Bread");
    }

    [Fact]
    public void UpdateItem_MissingItem_Throws()
    {
        var list = BuildList();
        var missingId = ListItemId.New();

        var act = () => list.UpdateItem(missingId, ListItemName.Create("X"), null, null, DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    // ── RemoveItem ──────────────────────────────────────────────────────────

    [Fact]
    public void RemoveItem_RemovesItemFromList()
    {
        var list = BuildList();
        var (id, name) = NewItem("Butter");
        list.AddItem(id, name, null, null, DateTime.UtcNow);
        list.ClearDomainEvents();

        list.RemoveItem(id, DateTime.UtcNow);

        list.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_EmitsListItemRemovedEvent()
    {
        var list = BuildList();
        var (id, name) = NewItem("Sugar");
        list.AddItem(id, name, null, null, DateTime.UtcNow);
        list.ClearDomainEvents();

        list.RemoveItem(id, DateTime.UtcNow);

        list.DomainEvents.Should().HaveCount(1);
        list.DomainEvents.Single().Should().BeOfType<ListItemRemoved>();
    }

    [Fact]
    public void RemoveItem_Event_HasCorrectItemId()
    {
        var list = BuildList();
        var (id, name) = NewItem("Salt");
        list.AddItem(id, name, null, null, DateTime.UtcNow);
        list.ClearDomainEvents();

        list.RemoveItem(id, DateTime.UtcNow);

        var evt = (ListItemRemoved)list.DomainEvents.Single();
        evt.ItemId.Should().Be(id.Value);
    }

    [Fact]
    public void RemoveItem_MissingItem_Throws()
    {
        var list = BuildList();
        var missingId = ListItemId.New();

        var act = () => list.RemoveItem(missingId, DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemoveItem_DecreasesItemCount()
    {
        var list = BuildList();
        var id1 = ListItemId.New();
        var id2 = ListItemId.New();
        list.AddItem(id1, ListItemName.Create("A"), null, null, DateTime.UtcNow);
        list.AddItem(id2, ListItemName.Create("B"), null, null, DateTime.UtcNow);

        list.RemoveItem(id1, DateTime.UtcNow);

        list.Items.Should().HaveCount(1);
        list.Items.Single().Name.Value.Should().Be("B");
    }

    // ── LinkToEntity ────────────────────────────────────────────────────────

    [Fact]
    public void LinkToEntity_SetsLinkedEntityTypeAndId()
    {
        var list = BuildList();
        var entityId = Guid.NewGuid();

        list.LinkToEntity("CalendarEvent", entityId, DateTime.UtcNow);

        list.LinkedEntityType.Should().Be("CalendarEvent");
        list.LinkedEntityId.Should().Be(entityId);
    }

    [Fact]
    public void LinkToEntity_EmitsListLinkedEvent()
    {
        var list = BuildList();
        var entityId = Guid.NewGuid();
        list.ClearDomainEvents(); // discard Create event

        list.LinkToEntity("CalendarEvent", entityId, DateTime.UtcNow);

        list.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ListLinked>()
            .Which.LinkedEntityType.Should().Be("CalendarEvent");
    }

    [Fact]
    public void LinkToEntity_ListLinkedEvent_HasCorrectEntityId()
    {
        var list = BuildList();
        var entityId = Guid.NewGuid();
        list.ClearDomainEvents();

        list.LinkToEntity("CalendarEvent", entityId, DateTime.UtcNow);

        var evt = list.DomainEvents.OfType<ListLinked>().Single();
        evt.LinkedEntityId.Should().Be(entityId);
        evt.ListId.Should().Be(list.Id.Value);
    }

    // ── Unlink ──────────────────────────────────────────────────────────────

    [Fact]
    public void Unlink_ClearsLinkedEntityTypeAndId()
    {
        var list = BuildList();
        list.LinkToEntity("CalendarEvent", Guid.NewGuid(), DateTime.UtcNow);

        list.Unlink(DateTime.UtcNow);

        list.LinkedEntityType.Should().BeNull();
        list.LinkedEntityId.Should().BeNull();
    }

    [Fact]
    public void Unlink_EmitsListUnlinkedEvent()
    {
        var list = BuildList();
        list.LinkToEntity("CalendarEvent", Guid.NewGuid(), DateTime.UtcNow);
        list.ClearDomainEvents();

        list.Unlink(DateTime.UtcNow);

        list.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ListUnlinked>()
            .Which.ListId.Should().Be(list.Id.Value);
    }

    // ── Rename ──────────────────────────────────────────────────────────────

    [Fact]
    public void Rename_UpdatesName()
    {
        var list = BuildList("Old Name");
        var newName = ListName.Create("New Name");

        list.Rename(newName, DateTime.UtcNow);

        list.Name.Value.Should().Be("New Name");
    }

    [Fact]
    public void Rename_EmitsListRenamedEvent()
    {
        var list = BuildList("Old Name");
        list.ClearDomainEvents();
        var newName = ListName.Create("New Name");

        list.Rename(newName, DateTime.UtcNow);

        list.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ListRenamed>()
            .Which.NewName.Should().Be("New Name");
    }

    [Fact]
    public void Rename_ListRenamedEvent_HasCorrectListId()
    {
        var list = BuildList("Old Name");
        list.ClearDomainEvents();

        list.Rename(ListName.Create("Updated"), DateTime.UtcNow);

        var evt = list.DomainEvents.OfType<ListRenamed>().Single();
        evt.ListId.Should().Be(list.Id.Value);
    }

    // ── Delete ──────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_EmitsListDeletedEvent()
    {
        var list = BuildList();
        list.ClearDomainEvents();

        list.Delete(DateTime.UtcNow);

        list.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ListDeleted>()
            .Which.ListId.Should().Be(list.Id.Value);
    }

    // ── ReorderItems ─────────────────────────────────────────────────────────

    [Fact]
    public void ReorderItems_AssignsNewOrdersToUncheckedItems()
    {
        var list = BuildList();
        var (id1, n1) = NewItem("A");
        var (id2, n2) = NewItem("B");
        var (id3, n3) = NewItem("C");
        list.AddItem(id1, n1, null, null, DateTime.UtcNow);
        list.AddItem(id2, n2, null, null, DateTime.UtcNow);
        list.AddItem(id3, n3, null, null, DateTime.UtcNow);

        list.ReorderItems([id3, id1, id2], DateTime.UtcNow);

        list.Items.Single(i => i.Id == id3).Order.Should().Be(1);
        list.Items.Single(i => i.Id == id1).Order.Should().Be(2);
        list.Items.Single(i => i.Id == id2).Order.Should().Be(3);
    }

    [Fact]
    public void ReorderItems_EmitsListItemsReorderedEvent()
    {
        var list = BuildList();
        var (id1, n1) = NewItem("X");
        var (id2, n2) = NewItem("Y");
        list.AddItem(id1, n1, null, null, DateTime.UtcNow);
        list.AddItem(id2, n2, null, null, DateTime.UtcNow);
        list.ClearDomainEvents();

        list.ReorderItems([id2, id1], DateTime.UtcNow);

        list.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ListItemsReordered>()
            .Which.ListId.Should().Be(list.Id.Value);
    }

    [Fact]
    public void ReorderItems_DoesNotAffectCheckedItems()
    {
        var list = BuildList();
        var (uncheckedId, un) = NewItem("Unchecked");
        var (checkedId, cn) = NewItem("Checked");
        list.AddItem(uncheckedId, un, null, null, DateTime.UtcNow);
        list.AddItem(checkedId, cn, null, null, DateTime.UtcNow);
        list.ToggleItem(checkedId, null, DateTime.UtcNow);

        // All items must be included; caller decides checked item position
        list.ReorderItems([checkedId, uncheckedId], DateTime.UtcNow);

        list.Items.Single(i => i.Id == checkedId).Order.Should().Be(1);
        list.Items.Single(i => i.Id == uncheckedId).Order.Should().Be(2);
    }

    [Fact]
    public void ReorderItems_WithWrongItemCount_ThrowsArgumentException()
    {
        var list = BuildList();
        var (id1, n1) = NewItem("One");
        var (id2, n2) = NewItem("Two");
        list.AddItem(id1, n1, null, null, DateTime.UtcNow);
        list.AddItem(id2, n2, null, null, DateTime.UtcNow);

        var act = () => list.ReorderItems([id1], DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReorderItems_WithDuplicateIds_ThrowsArgumentException()
    {
        var list = BuildList();
        var (id1, n1) = NewItem("One");
        list.AddItem(id1, n1, null, null, DateTime.UtcNow);

        var act = () => list.ReorderItems([id1, id1], DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReorderItems_WithUnknownItemId_ThrowsInvalidOperationException()
    {
        var list = BuildList();
        var (id1, n1) = NewItem("One");
        list.AddItem(id1, n1, null, null, DateTime.UtcNow);
        var unknownId = ListItemId.New();

        var act = () => list.ReorderItems([unknownId], DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ReorderItems_OrderRemainsStableAfterMixedOperations()
    {
        var list = BuildList();
        var (id1, n1) = NewItem("A");
        var (id2, n2) = NewItem("B");
        var (id3, n3) = NewItem("C");
        list.AddItem(id1, n1, null, null, DateTime.UtcNow);
        list.AddItem(id2, n2, null, null, DateTime.UtcNow);
        list.AddItem(id3, n3, null, null, DateTime.UtcNow);

        // Toggle one checked; full reorder must include all items
        list.ToggleItem(id2, null, DateTime.UtcNow);
        list.ReorderItems([id3, id2, id1], DateTime.UtcNow);

        list.Items.Single(i => i.Id == id3).Order.Should().Be(1);
        list.Items.Single(i => i.Id == id2).Order.Should().Be(2);
        list.Items.Single(i => i.Id == id1).Order.Should().Be(3);
    }

    // ── SetItemTemporal — repeat invariant ─────────────────────────────────

    [Fact]
    public void SetItemTemporal_WithRepeatOnly_DoesNotThrow()
    {
        var list = BuildList();
        var (id, name) = NewItem("Recurring task");
        list.AddItem(id, name, null, null, DateTime.UtcNow);

        // Repeat may be set independently — dueDate is not required
        var act = () => list.SetItemTemporal(id, null, null, "weekly", DateTime.UtcNow);

        act.Should().NotThrow();
    }

    [Fact]
    public void SetItemTemporal_WithRepeatOnly_SetsRepeatField()
    {
        var list = BuildList();
        var (id, name) = NewItem("Recurring task");
        list.AddItem(id, name, null, null, DateTime.UtcNow);

        list.SetItemTemporal(id, null, null, "weekly", DateTime.UtcNow);

        var item = list.Items.Single(i => i.Id == id);
        item.Repeat.Should().Be("weekly");
        item.DueDate.Should().BeNull();
        item.Reminder.Should().BeNull();
    }

    [Fact]
    public void SetItemTemporal_WithRepeatOnly_ItemHasTemporalData()
    {
        var list = BuildList();
        var (id, name) = NewItem("Recurring task");
        list.AddItem(id, name, null, null, DateTime.UtcNow);

        list.SetItemTemporal(id, null, null, "weekly", DateTime.UtcNow);

        list.Items.Single(i => i.Id == id).HasTemporalData.Should().BeTrue();
    }

    [Fact]
    public void SetItemTemporal_WithDueDateCleared_AndRepeatRemains_ItemIsStillTemporalEligible()
    {
        var list = BuildList();
        var (id, name) = NewItem("Task with due date and repeat");
        list.AddItem(id, name, null, null, DateTime.UtcNow);
        list.SetItemTemporal(id, new DateOnly(2026, 4, 11), null, "weekly", DateTime.UtcNow);

        // Now clear just the dueDate (simulate clearing it leaving repeat)
        // SetTemporal is additive — use ClearTemporal then re-set repeat only
        list.ClearItemTemporal(id, DateTime.UtcNow);
        list.SetItemTemporal(id, null, null, "weekly", DateTime.UtcNow);

        var item = list.Items.Single(i => i.Id == id);
        item.DueDate.Should().BeNull();
        item.Repeat.Should().Be("weekly");
        item.HasTemporalData.Should().BeTrue();
    }
}

