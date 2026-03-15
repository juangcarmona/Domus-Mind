namespace DomusMind.Contracts.Responsibilities;

public sealed record OwnerSuggestion(
    Guid MemberId,
    string MemberName,
    int CurrentPrimaryOwnershipCount);

public sealed record SuggestResponsibilityOwnerResponse(
    Guid ResponsibilityDomainId,
    IReadOnlyCollection<OwnerSuggestion> Suggestions);
