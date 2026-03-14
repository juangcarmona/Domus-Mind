namespace DomusMind.Contracts.Auth;

public sealed record MeResponse(Guid UserId, string? Email);
