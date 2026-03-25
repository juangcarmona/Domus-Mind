using DomusMind.Domain.Family;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.SharedLists;
using DomusMind.Domain.SharedLists.Events;
using DomusMind.Domain.SharedLists.ValueObjects;
using FluentAssertions;

namespace DomusMind.Domain.Tests.SharedLists;

public sealed class SharedListTests
{
    private static SharedList BuildList(
        string name = "Weekly Shopping",
        string kind = "Shopping",
        FamilyId? familyId = null)
    {
        return SharedList.Create(
            SharedListId.New(),
            familyId ?? FamilyId.New(),
            SharedListName.Create(name),
            SharedListKind.Create(kind),
            areaId: null,
            linkedEntityType: null,
            linkedEntityId: null,
            createdAtUtc: DateTime.UtcNow);
    }

    private static (SharedListItemId, SharedListItemName) NewItem(string name = "Milk")
        => (SharedListItemId.New(), SharedListItemName.Create(name));

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
    public void Create_EmitsSharedListCreatedEvent()
    {
        var list = BuildList();

        list.DomainEvents.Should().HaveCount(1);
        list.DomainEvents.Single().Should().BeOfType<SharedListCreated>();
    }

    [Fact]
    public void Create_SharedListCreatedEvent_HasCorrectFamilyId()
    {
        var familyId = FamilyId.New();
        var list = BuildList(familyId: familyId);

        var evt = (SharedListCreated)list.DomainEvents.Single();
        evt.FamilyId.Should().Be(familyId.Value);
    }

    [Fact]
    public void Create_WithAreaId_SetsAreaId()
    {
        var areaId = ResponsibilityDomainId.New();
        var list = SharedList.Create(
            SharedListId.New(), FamilyId.New(),
            SharedListName.Create("Chores"), SharedListKind.Create("General"),
            areaId, null, null, DateTime.UtcNow);

        list.AreaId.Should().Be(areaId);
    }

    // ── SharedListName validation ───────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SharedListName_Create_WithEmptyValue_Throws(string name)
    {
        var act = () => SharedListName.Create(name);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SharedListName_Create_ExceedingMaxLength_Throws()
    {
        var tooLong = new string('x', SharedListName.MaxLength + 1);

        var act = () => SharedListName.Create(tooLong);

        act.Should().Throw<ArgumentException>();
    }

    // ── SharedListItemName validation ───────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SharedListItemName_Create_WithEmptyValue_Throws(string name)
    {
        var act = () => SharedListItemName.Create(name);

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

        list.AddItem(SharedListItemId.New(), SharedListItemName.Create("Apples"), null, null, DateTime.UtcNow);
        list.AddItem(SharedListItemId.New(), SharedListItemName.Create("Bananas"), null, null, DateTime.UtcNow);
        list.AddItem(SharedListItemId.New(), SharedListItemName.Create("Cherries"), null, null, DateTime.UtcNow);

        list.Items.Select(i => i.Order).Should().BeEquivalentTo([1, 2, 3]);
    }

    [Fact]
    public void AddItem_EmitsSharedListItemAddedEvent()
    {
        var list = BuildList();
        list.ClearDomainEvents();
        var (id, name) = NewItem("Milk");

        list.AddItem(id, name, null, null, DateTime.UtcNow);

        list.DomainEvents.Should().HaveCount(1);
        list.DomainEvents.Single().Should().BeOfType<SharedListItemAdded>();
    }

    [Fact]
    public void AddItem_Event_HasCorrectItemName()
    {
        var list = BuildList();
        list.ClearDomainEvents();
        var (id, name) = NewItem("Cheese");

        list.AddItem(id, name, null, null, DateTime.UtcNow);

        var evt = (SharedListItemAdded)list.DomainEvents.Single();
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
    public void ToggleItem_EmitsSharedListItemToggledEvent()
    {
        var list = BuildList();
        var (id, name) = NewItem("Yogurt");
        list.AddItem(id, name, null, null, DateTime.UtcNow);
        list.ClearDomainEvents();

        list.ToggleItem(id, null, DateTime.UtcNow);

        list.DomainEvents.Should().HaveCount(1);
        list.DomainEvents.Single().Should().BeOfType<SharedListItemToggled>();
    }

    [Fact]
    public void ToggleItem_Event_ReflectsNewCheckedState()
    {
        var list = BuildList();
        var (id, name) = NewItem("Cream");
        list.AddItem(id, name, null, null, DateTime.UtcNow);
        list.ClearDomainEvents();

        list.ToggleItem(id, null, DateTime.UtcNow);

        var evt = (SharedListItemToggled)list.DomainEvents.Single();
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
        var missingId = SharedListItemId.New();

        var act = () => list.ToggleItem(missingId, null, DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    // ── UncheckedCount ──────────────────────────────────────────────────────

    [Fact]
    public void UncheckedCount_ReturnsNumberOfUncheckedItems()
    {
        var list = BuildList();
        var id1 = SharedListItemId.New();
        var id2 = SharedListItemId.New();
        var id3 = SharedListItemId.New();
        list.AddItem(id1, SharedListItemName.Create("A"), null, null, DateTime.UtcNow);
        list.AddItem(id2, SharedListItemName.Create("B"), null, null, DateTime.UtcNow);
        list.AddItem(id3, SharedListItemName.Create("C"), null, null, DateTime.UtcNow);

        list.ToggleItem(id1, null, DateTime.UtcNow);

        list.UncheckedCount.Should().Be(2);
    }

    [Fact]
    public void UncheckedCount_EmptyList_ReturnsZero()
    {
        var list = BuildList();

        list.UncheckedCount.Should().Be(0);
    }
}
