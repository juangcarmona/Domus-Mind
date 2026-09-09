using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists.ValueObjects;

namespace DomusMind.Domain.Lists;

/// <summary>
/// An individual item within a shared list.
/// Internal entity of the SharedList aggregate.
/// Items are polymorphic execution units supporting base, importance, and temporal capabilities.
/// </summary>
public sealed class ListItem : Entity<ListItemId>
{
    public ListItemName Name { get; private set; }
    public bool Checked { get; private set; }
    public string? Quantity { get; private set; }
    public string? Note { get; private set; }
    public int Order { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public MemberId? UpdatedByMemberId { get; private set; }

    public bool Importance { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public DateTimeOffset? Reminder { get; private set; }
    public string? Repeat { get; private set; }

    /// <summary>Optional area this item is associated with (lightweight item-level context, not list ownership).</summary>
    public Guid? ItemAreaId { get; private set; }
    /// <summary>Optional target member for this item (who it's for — lightweight metadata, not task ownership).</summary>
    public Guid? TargetMemberId { get; private set; }

    public bool HasTemporalData => DueDate.HasValue || Reminder.HasValue || Repeat is not null;

    private ListItem(
        ListItemId id,
        ListItemName name,
        string? quantity,
        string? note,
        int order,
        DateTime createdAtUtc)
        : base(id)
    {
        Name = name;
        Checked = false;
        Quantity = quantity;
        Note = note;
        Order = order;
        UpdatedAtUtc = createdAtUtc;
        Importance = false;
    }

    // Required by EF Core for materialization
    private ListItem() : base(default)
    {
        Name = null!;
    }

    internal static ListItem Create(
        ListItemId id,
        ListItemName name,
        string? quantity,
        string? note,
        int order,
        DateTime createdAtUtc)
        => new(id, name, quantity, note, order, createdAtUtc);

    internal void Toggle(MemberId? updatedByMemberId, DateTime updatedAtUtc)
    {
        Checked = !Checked;
        UpdatedByMemberId = updatedByMemberId;
        UpdatedAtUtc = updatedAtUtc;
    }

    internal void Update(ListItemName name, string? quantity, string? note, DateTime updatedAtUtc)
    {
        Name = name;
        Quantity = quantity;
        Note = note;
        UpdatedAtUtc = updatedAtUtc;
    }

    internal void SetOrder(int order)
    {
        Order = order;
    }

    internal void SetImportance(bool importance, DateTime updatedAtUtc)
    {
        Importance = importance;
        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>
    /// Sets temporal fields. At least one must be provided.
    /// All temporal fields are independently optional; repeat does not require due date.
    /// Returns whether this is the first time temporal data is being set (transition event).
    /// </summary>
    internal bool SetTemporal(
        DateOnly? dueDate,
        DateTimeOffset? reminder,
        string? repeat,
        DateTime updatedAtUtc)
    {
        var wasTemporalBefore = HasTemporalData;

        if (dueDate.HasValue) DueDate = dueDate;
        if (reminder.HasValue) Reminder = reminder;
        if (repeat is not null) Repeat = repeat;

        UpdatedAtUtc = updatedAtUtc;

        return !wasTemporalBefore && HasTemporalData;
    }

    /// <summary>
    /// Clears all temporal fields atomically.
    /// Returns true if the item had temporal data before the clear.
    /// </summary>
    internal bool ClearTemporal(DateTime updatedAtUtc)
    {
        var hadTemporalData = HasTemporalData;
        DueDate = null;
        Reminder = null;
        Repeat = null;
        UpdatedAtUtc = updatedAtUtc;
        return hadTemporalData;
    }

    internal void SetContext(Guid? itemAreaId, Guid? targetMemberId, DateTime updatedAtUtc)
    {
        ItemAreaId = itemAreaId;
        TargetMemberId = targetMemberId;
        UpdatedAtUtc = updatedAtUtc;
    }
}

