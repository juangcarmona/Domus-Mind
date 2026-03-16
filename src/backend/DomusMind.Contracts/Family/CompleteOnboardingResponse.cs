namespace DomusMind.Contracts.Family;

public sealed record OnboardingMemberItem(
    Guid MemberId,
    string Name,
    string Role,
    bool IsManager,
    DateOnly? BirthDate,
    DateTime JoinedAtUtc);

public sealed record CompleteOnboardingResponse(
    Guid FamilyId,
    string FamilyName,
    IReadOnlyCollection<OnboardingMemberItem> Members);
