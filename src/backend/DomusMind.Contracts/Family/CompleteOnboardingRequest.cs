namespace DomusMind.Contracts.Family;

public sealed record AdditionalMemberRequest(
    string Name,
    DateOnly? BirthDate,
    string? Type,
    bool Manager = false);

public sealed record CompleteOnboardingRequest(
    string SelfName,
    DateOnly? SelfBirthDate,
    IReadOnlyCollection<AdditionalMemberRequest>? AdditionalMembers);
