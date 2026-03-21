namespace DomusMind.Contracts.Calendar;

public sealed record FamilyPlanItem(
    Guid EventId,
    string Title,
    string Date,
    string? Time,
    string? EndDate,
    string? EndTime,
    string Status,
    string Color,
    IReadOnlyCollection<Guid> ParticipantMemberIds,
    IReadOnlyCollection<ParticipantProjection> Participants);

public sealed record FamilyPlansResponse(
    IReadOnlyCollection<FamilyPlanItem> Plans);
