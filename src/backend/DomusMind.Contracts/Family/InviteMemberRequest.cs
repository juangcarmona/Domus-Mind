namespace DomusMind.Contracts.Family;

public sealed record InviteMemberRequest(
    string Name,
    string Role,
    DateOnly? BirthDate,
    bool IsManager,
    string Username,
    string TemporaryPassword);
