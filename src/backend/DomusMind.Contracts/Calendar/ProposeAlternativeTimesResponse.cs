namespace DomusMind.Contracts.Calendar;

public sealed record AlternativeTimeSlot(
    DateTime ProposedStart,
    DateTime ProposedEnd);

public sealed record ProposeAlternativeTimesResponse(
    Guid EventId,
    IReadOnlyCollection<AlternativeTimeSlot> Suggestions);
