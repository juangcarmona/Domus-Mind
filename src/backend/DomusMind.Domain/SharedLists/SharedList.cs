using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.SharedLists.Events;
using DomusMind.Domain.SharedLists.ValueObjects;

namespace DomusMind.Domain.SharedLists;

/// <summary>
/// Aggregate root for the Shared Lists bounded context.
/// Represents a persistent reusable shared checklist owned by a family.
/// </summary>
public sealed class SharedList : AggregateRoot<SharedListId>
{
    private readonly List<SharedListItem> _items = [];

    public FamilyId FamilyId { get; private set; }
    public SharedListName Name { get; private set; }
    public SharedListKind Kind { get; private set; }
    public ResponsibilityDomainId? AreaId { get; private set; }
    public string? LinkedEntityType { get; private set; }
    public Guid? LinkedEntityId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<SharedListItem> Items => _items.AsReadOnly();

    private SharedList(
        SharedListId id,
        FamilyId familyId,
        SharedListName name,
        SharedListKind kind,
        ResponsibilityDomainId? areaId,
        string? linkedEntityType,
        Guid? linkedEntityId,
        DateTime createdAtUtc)
        : base(id)
    {
        FamilyId = familyId;
        Name = name;
        Kind = kind;
        AreaId = areaId;
        LinkedEntityType = linkedEntityType;
        LinkedEntityId = linkedEntityId;
        CreatedAtUtc = createdAtUtc;
    }

    // Required by EF Core for materialization
    private SharedList() : base(default)
    {
        Name = null!;
        Kind = null!;
    }

    public static SharedList Create(
        SharedListId id,
        FamilyId familyId,
        SharedListName name,
        SharedListKind kind,
        ResponsibilityDomainId? areaId,
        string? linkedEntityType,
        Guid? linkedEntityId,
        DateTime createdAtUtc)
    {
        var list = new SharedList(id, familyId, name, kind, areaId, linkedEntityType, linkedEntityId, createdAtUtc);
        list.RaiseDomainEvent(new SharedListCreated(
            Guid.NewGuid(), id.Value, familyId.Value, name.Value, kind.Value, createdAtUtc));
        return list;
    }

    public SharedListItem AddItem(
        SharedListItemId itemId,
        SharedListItemName name,
        string? quantity,
        string? note,
        DateTime now)
    {
        var order = _items.Count + 1;
        var item = SharedListItem.Create(itemId, name, quantity, note, order, now);
        _items.Add(item);

        RaiseDomainEvent(new SharedListItemAdded(
            Guid.NewGuid(), Id.Value, itemId.Value, name.Value, order, now));

        return item;
    }

    public SharedListItem ToggleItem(
        SharedListItemId itemId,
        MemberId? updatedByMemberId,
        DateTime now)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException($"Item '{itemId.Value}' does not exist in this list.");

        item.Toggle(updatedByMemberId, now);

        RaiseDomainEvent(new SharedListItemToggled(
            Guid.NewGuid(), Id.Value, itemId.Value, item.Checked, now));

        return item;
    }

    public SharedListItem UpdateItem(
        SharedListItemId itemId,
        SharedListItemName name,
        string? quantity,
        string? note,
        DateTime now)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException($"Item '{itemId.Value}' does not exist in this list.");

        item.Update(name, quantity, note, now);

        RaiseDomainEvent(new SharedListItemUpdated(
            Guid.NewGuid(), Id.Value, itemId.Value, name.Value, quantity, note, now));

        return item;
    }

    public void RemoveItem(SharedListItemId itemId, DateTime now)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException($"Item '{itemId.Value}' does not exist in this list.");

        _items.Remove(item);

        RaiseDomainEvent(new SharedListItemRemoved(
            Guid.NewGuid(), Id.Value, itemId.Value, now));
    }

    public void LinkToEntity(string entityType, Guid entityId, DateTime now)
    {
        LinkedEntityType = entityType;
        LinkedEntityId = entityId;
        RaiseDomainEvent(new SharedListLinked(Guid.NewGuid(), Id.Value, entityType, entityId, now));
    }

    public void Unlink(DateTime now)
    {
        LinkedEntityType = null;
        LinkedEntityId = null;
        RaiseDomainEvent(new SharedListUnlinked(Guid.NewGuid(), Id.Value, now));
    }

    public void Rename(SharedListName newName, DateTime now)
    {
        Name = newName;
        RaiseDomainEvent(new SharedListRenamed(Guid.NewGuid(), Id.Value, newName.Value, now));
    }

    public void Delete(DateTime now)
    {
        RaiseDomainEvent(new SharedListDeleted(Guid.NewGuid(), Id.Value, now));
    }

    /// <summary>
    /// Reorders unchecked items within the list. The provided list must contain
    /// exactly all currently unchecked item IDs. Checked items are not affected.
    /// </summary>
    public void ReorderItems(IReadOnlyList<SharedListItemId> orderedUncheckedIds, DateTime now)
    {
        var uncheckedItems = _items.Where(i => !i.Checked).ToList();

        if (orderedUncheckedIds.Count != uncheckedItems.Count)
            throw new ArgumentException(
                $"Reorder payload must include exactly all {uncheckedItems.Count} unchecked items.");

        var distinctIds = orderedUncheckedIds.ToHashSet();
        if (distinctIds.Count != orderedUncheckedIds.Count)
            throw new ArgumentException("Duplicate item IDs in reorder payload.");

        foreach (var item in uncheckedItems)
        {
            if (!distinctIds.Contains(item.Id))
                throw new InvalidOperationException(
                    $"Unchecked item '{item.Id.Value}' is missing from the reorder payload.");
        }

        for (var i = 0; i < orderedUncheckedIds.Count; i++)
        {
            var item = _items.Single(it => it.Id == orderedUncheckedIds[i]);
            item.SetOrder(i + 1);
        }

        var entries = orderedUncheckedIds
            .Select((id, i) => new ItemOrderEntry(id.Value, i + 1))
            .ToList();
        RaiseDomainEvent(new SharedListItemsReordered(Guid.NewGuid(), Id.Value, entries, now));
    }

    /// <summary>Count of items that are not yet checked.</summary>
    public int UncheckedCount => _items.Count(i => !i.Checked);
}
