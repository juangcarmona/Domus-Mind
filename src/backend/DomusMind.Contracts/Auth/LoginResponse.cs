namespace DomusMind.Contracts.Auth;

public sealed record LoginResponse(string AccessToken, string RefreshToken, Guid UserId, string Email, bool MustChangePassword);
