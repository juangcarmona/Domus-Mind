namespace DomusMind.Contracts.Platform;

public sealed record DeploymentModeResponse(
    string DeploymentMode,
    bool CanCreateHousehold,
    bool RequiresInvitation,
    bool SupportsEmail,
    bool SupportsAdminTools);
