namespace DomusMind.Contracts.Calendar;

public sealed record ParticipantSuggestion(
    Guid MemberId,
    string MemberName,
    int PastParticipationCount);

public sealed record SuggestEventParticipantsResponse(
    Guid EventId,
    IReadOnlyCollection<ParticipantSuggestion> Suggestions);
