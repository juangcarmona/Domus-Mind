namespace DomusMind.Contracts.Family;

public sealed record AddMemberRequest(string Name, string Role, DateOnly? BirthDate = null, bool IsManager = false);
