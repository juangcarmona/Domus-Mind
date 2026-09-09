using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.Lists.Events;
using DomusMind.Domain.Lists.ValueObjects;

namespace DomusMind.Domain.Lists;

/// <summary>
/// Aggregate root for the Shared Lists bounded context.
/// Represents a persistent reusable shared checklist owned by a family.
/// </summary>
public sealed class SharedList : AggregateRoot<ListId>
{
    private readonly List<ListItem> _items = [];

    public FamilyId FamilyId { get; private set; }
    public ListName Name { get; private set; }
    public ListKind Kind { get; private set; }
    /// <summary>Nullable hex color string (e.g. "#A9BCF5"). Null means no color override.</summary>
    public string? Color { get; private set; }
    public ResponsibilityDomainId? AreaId { get; private set; }
    public string? LinkedEntityType { get; private set; }
    public Guid? LinkedEntityId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public bool IsArchived { get; private set; }

    public IReadOnlyCollection<ListItem> Items => _items.AsReadOnly();

    private SharedList(
        ListId id,
        FamilyId familyId,
        ListName name,
        ListKind kind,
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
        IsArchived = false;
    }

    // Required by EF Core for materialization
    private SharedList() : base(default)
    {
        Name = null!;
        Kind = null!;
    }

    public static SharedList Create(
        ListId id,
        FamilyId familyId,
        ListName name,
        ListKind kind,
        ResponsibilityDomainId? areaId,
        string? linkedEntityType,
        Guid? linkedEntityId,
        DateTime createdAtUtc)
    {
        var list = new SharedList(id, familyId, name, kind, areaId, linkedEntityType, linkedEntityId, createdAtUtc);
        list.RaiseDomainEvent(new ListCreated(
            Guid.NewGuid(), id.Value, familyId.Value, name.Value, kind.Value, createdAtUtc));
        return list;
    }

    public ListItem AddItem(
        ListItemId itemId,
        ListItemName name,
        string? quantity,
        string? note,
        DateTime now)
    {
        var order = _items.Count + 1;
        var item = ListItem.Create(itemId, name, quantity, note, order, now);
        _items.Add(item);

        RaiseDomainEvent(new ListItemAdded(
            Guid.NewGuid(), Id.Value, itemId.Value, name.Value, order, now));

        return item;
    }

    public ListItem ToggleItem(
        ListItemId itemId,
        MemberId? updatedByMemberId,
        DateTime now)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException($"Item '{itemId.Value}' does not exist in this list.");

        item.Toggle(updatedByMemberId, now);

        RaiseDomainEvent(new ListItemToggled(
            Guid.NewGuid(), Id.Value, itemId.Value, item.Checked, now));

        return item;
    }

    public ListItem UpdateItem(
        ListItemId itemId,
        ListItemName name,
        string? quantity,
        string? note,
        DateTime now)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException($"Item '{itemId.Value}' does not exist in this list.");

        item.Update(name, quantity, note, now);

        RaiseDomainEvent(new ListItemUpdated(
            Guid.NewGuid(), Id.Value, itemId.Value, name.Value, quantity, note, now));

        return item;
    }

    public void RemoveItem(ListItemId itemId, DateTime now)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException($"Item '{itemId.Value}' does not exist in this list.");

        _items.Remove(item);

        RaiseDomainEvent(new ListItemRemoved(
            Guid.NewGuid(), Id.Value, itemId.Value, now));
    }

    public void LinkToEntity(string entityType, Guid entityId, DateTime now)
    {
        LinkedEntityType = entityType;
        LinkedEntityId = entityId;
        RaiseDomainEvent(new ListLinked(Guid.NewGuid(), Id.Value, entityType, entityId, now));
    }

    public void Unlink(DateTime now)
    {
        LinkedEntityType = null;
        LinkedEntityId = null;
        RaiseDomainEvent(new ListUnlinked(Guid.NewGuid(), Id.Value, now));
    }

    public void Rename(ListName newName, DateTime now)
    {
        Name = newName;
        RaiseDomainEvent(new ListRenamed(Guid.NewGuid(), Id.Value, newName.Value, now));
    }

    /// <summary>
    /// Updates list-level metadata. Any combination of fields may be provided.
    /// Fields left null are not changed. Updating metadata does not affect list semantics
    /// or its item model.
    /// </summary>
    public void UpdateMetadata(
        ListName? newName,
        ResponsibilityDomainId? newAreaId,
        bool clearArea,
        string? newLinkedEntityType,
        Guid? newLinkedEntityId,
        bool clearLink,
        ListKind? newKind,
        DateTime now)
    {
        if (newName is not null)
        {
            Name = newName;
            RaiseDomainEvent(new ListRenamed(Guid.NewGuid(), Id.Value, newName.Value, now));
        }

        if (clearArea)
            AreaId = null;
        else if (newAreaId is not null)
            AreaId = newAreaId;

        if (clearLink)
        {
            LinkedEntityType = null;
            LinkedEntityId = null;
            RaiseDomainEvent(new ListUnlinked(Guid.NewGuid(), Id.Value, now));
        }
        else if (newLinkedEntityType is not null && newLinkedEntityId.HasValue)
        {
            LinkedEntityType = newLinkedEntityType;
            LinkedEntityId = newLinkedEntityId;
            RaiseDomainEvent(new ListLinked(Guid.NewGuid(), Id.Value, newLinkedEntityType, newLinkedEntityId.Value, now));
        }

        if (newKind is not null)
            Kind = newKind;
    }

    public void SetColor(string? color, DateTime now)
    {
        Color = color;
    }

    public void Delete(DateTime now)
    {
        RaiseDomainEvent(new ListDeleted(Guid.NewGuid(), Id.Value, now));
    }

    public ListItem SetItemContext(
        ListItemId itemId,
        Guid? itemAreaId,
        Guid? targetMemberId,
        DateTime now)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException($"Item '{itemId.Value}' does not exist in this list.");

        item.SetContext(itemAreaId, targetMemberId, now);
        return item;
    }

    public void Archive(DateTime now)
    {
        if (IsArchived)
            throw new InvalidOperationException("List is already archived.");

        IsArchived = true;
        RaiseDomainEvent(new ListArchived(Guid.NewGuid(), Id.Value, now));
    }

    public void Restore(DateTime now)
    {
        if (!IsArchived)
            throw new InvalidOperationException("List is not archived.");

        IsArchived = false;
        RaiseDomainEvent(new ListRestored(Guid.NewGuid(), Id.Value, now));
    }

    /// <summary>
    /// Reorders items within the list. The provided list must contain
    /// exactly all item IDs belonging to this list in the desired order.
    /// Order is for human scanning convenience and carries no semantic meaning.
    /// </summary>
    public void ReorderItems(IReadOnlyList<ListItemId> orderedItemIds, DateTime now)
    {
        if (orderedItemIds.Count != _items.Count)
            throw new ArgumentException(
                $"Reorder payload must include exactly all {_items.Count} items.");

        var distinctIds = orderedItemIds.ToHashSet();
        if (distinctIds.Count != orderedItemIds.Count)
            throw new ArgumentException("Duplicate item IDs in reorder payload.");

        foreach (var item in _items)
        {
            if (!distinctIds.Contains(item.Id))
                throw new InvalidOperationException(
                    $"Item '{item.Id.Value}' is missing from the reorder payload.");
        }

        for (var i = 0; i < orderedItemIds.Count; i++)
        {
            var item = _items.Single(it => it.Id == orderedItemIds[i]);
            item.SetOrder(i + 1);
        }

        var entries = orderedItemIds
            .Select((id, i) => new ItemOrderEntry(id.Value, i + 1))
            .ToList();
        RaiseDomainEvent(new ListItemsReordered(Guid.NewGuid(), Id.Value, entries, now));
    }

    /// <summary>Count of items that are not yet checked.</summary>
    public int UncheckedCount => _items.Count(i => !i.Checked);

    public ListItem SetItemImportance(ListItemId itemId, bool importance, DateTime now)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException($"Item '{itemId.Value}' does not exist in this list.");

        item.SetImportance(importance, now);

        RaiseDomainEvent(new ListItemImportanceSet(
            Guid.NewGuid(), Id.Value, itemId.Value, importance, now));

        return item;
    }

    /// <summary>
    /// Sets temporal fields (dueDate, reminder, repeat) on an item.
    /// At least one field must be provided.
    /// All three fields are independently optional — repeat does not require dueDate.
    /// </summary>
    public ListItem SetItemTemporal(
        ListItemId itemId,
        DateOnly? dueDate,
        DateTimeOffset? reminder,
        string? repeat,
        DateTime now)
    {
        if (dueDate is null && reminder is null && repeat is null)
            throw new ArgumentException(
                "At least one temporal field (dueDate, reminder, repeat) must be provided.");

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException($"Item '{itemId.Value}' does not exist in this list.");

        var isFirstTemporalSet = item.SetTemporal(dueDate, reminder, repeat, now);

        if (isFirstTemporalSet)
        {
            RaiseDomainEvent(new ListItemScheduled(
                Guid.NewGuid(), Id.Value, itemId.Value,
                item.DueDate, item.Reminder, item.Repeat, now));
        }
        else
        {
            RaiseDomainEvent(new ListItemUpdated(
                Guid.NewGuid(), Id.Value, itemId.Value,
                item.Name.Value, item.Quantity, item.Note, now));
        }

        return item;
    }

    /// <summary>
    /// Clears all temporal fields from an item atomically.
    /// Emits ListItemScheduled (with nulls) to signal Agenda projection invalidation.
    /// </summary>
    public ListItem ClearItemTemporal(ListItemId itemId, DateTime now)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException($"Item '{itemId.Value}' does not exist in this list.");

        var hadTemporalData = item.ClearTemporal(now);

        if (hadTemporalData)
        {
            RaiseDomainEvent(new ListItemScheduled(
                Guid.NewGuid(), Id.Value, itemId.Value,
                null, null, null, now));
        }

        return item;
    }
}
