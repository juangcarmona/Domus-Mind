using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family;
using DomusMind.Domain.SharedLists.ValueObjects;

namespace DomusMind.Domain.SharedLists;

/// <summary>
/// An individual item within a shared list.
/// Internal entity of the SharedList aggregate.
/// </summary>
public sealed class SharedListItem : Entity<SharedListItemId>
{
    public SharedListItemName Name { get; private set; }
    public bool Checked { get; private set; }
    public string? Quantity { get; private set; }
    public string? Note { get; private set; }
    public int Order { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public MemberId? UpdatedByMemberId { get; private set; }

    private SharedListItem(
        SharedListItemId id,
        SharedListItemName name,
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
    }

    // Required by EF Core for materialization
    private SharedListItem() : base(default)
    {
        Name = null!;
    }

    internal static SharedListItem Create(
        SharedListItemId id,
        SharedListItemName name,
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

    internal void Update(SharedListItemName name, string? quantity, string? note, DateTime updatedAtUtc)
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
}
