namespace DomusMind.Contracts.Family;

public sealed record AdditionalMemberRequest(
    string Name,
    DateTime? BirthDate,
    string? Type,
    bool Manager = false);

public sealed record CompleteOnboardingRequest(
    string SelfName,
    DateTime? SelfBirthDate,
    IReadOnlyCollection<AdditionalMemberRequest>? AdditionalMembers);
