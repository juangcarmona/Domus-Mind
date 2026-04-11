namespace DomusMind.Contracts.Lists;

public sealed record SetItemTemporalRequest(
    DateOnly? DueDate,
    DateTimeOffset? Reminder,
    string? Repeat);

public sealed record SetItemTemporalResponse(
    Guid ItemId,
    DateOnly? DueDate,
    DateTimeOffset? Reminder,
    string? Repeat,
    DateTime UpdatedAtUtc);
