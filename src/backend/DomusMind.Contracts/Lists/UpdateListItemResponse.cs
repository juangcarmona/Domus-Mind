namespace DomusMind.Contracts.Lists;

public sealed record UpdateListItemResponse(
    Guid ItemId,
    string Name,
    string? Quantity,
    string? Note,
    DateTime UpdatedAtUtc,
    bool Importance,
    DateOnly? DueDate,
    DateTimeOffset? Reminder,
    string? Repeat);
