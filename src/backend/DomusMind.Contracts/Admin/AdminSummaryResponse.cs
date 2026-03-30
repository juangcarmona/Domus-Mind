namespace DomusMind.Contracts.Admin;

public sealed record AdminSummaryResponse(
    string DeploymentMode,
    int HouseholdCount,
    int UserCount,
    int PendingInvitationCount,
    bool IsSystemInitialized,
    // Effective deployment policy — safe for operator diagnostics
    bool AllowHouseholdCreation,
    bool InvitationsEnabled,
    bool RequireInvitationForSignup,
    bool AdminToolsEnabled,
    int MaxHouseholdsPerDeployment);
