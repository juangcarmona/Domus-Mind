namespace DomusMind.Contracts.Lists;

public sealed record AddItemToListResponse(
    Guid ItemId,
    Guid ListId,
    string Name,
    bool Checked,
    string? Quantity,
    string? Note,
    int Order,
    bool Importance,
    DateOnly? DueDate,
    DateTimeOffset? Reminder,
    string? Repeat);
