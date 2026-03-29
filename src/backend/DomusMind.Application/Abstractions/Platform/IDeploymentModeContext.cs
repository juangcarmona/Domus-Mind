namespace DomusMind.Application.Abstractions.Platform;

/// <summary>
/// Exposes the active deployment mode and effective runtime capabilities.
/// Resolved from configuration at startup. Read-only.
/// </summary>
public interface IDeploymentModeContext
{
    DeploymentMode Mode { get; }
    bool CanCreateHousehold { get; }
    bool InvitationsEnabled { get; }
    bool EmailEnabled { get; }
    bool SupportsAdminTools { get; }
}
