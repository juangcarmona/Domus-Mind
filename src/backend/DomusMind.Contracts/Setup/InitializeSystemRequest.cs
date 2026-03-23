namespace DomusMind.Contracts.Setup;

public sealed record InitializeSystemRequest(
    string Email,
    string Password,
    string? DisplayName = null);
