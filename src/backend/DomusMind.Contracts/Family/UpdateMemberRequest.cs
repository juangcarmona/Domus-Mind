namespace DomusMind.Contracts.Family;

public sealed record UpdateMemberRequest(
    string Name,
    string Role,
    DateOnly? BirthDate,
    bool IsManager);
