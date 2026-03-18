namespace DomusMind.Contracts.Calendar;

public sealed record AlternativeTimeSlot(
    string Date,
    string Time,
    string EndDate,
    string EndTime);

public sealed record ProposeAlternativeTimesResponse(
    Guid EventId,
    IReadOnlyCollection<AlternativeTimeSlot> Suggestions);
