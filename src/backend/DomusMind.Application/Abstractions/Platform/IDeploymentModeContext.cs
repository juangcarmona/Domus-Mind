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
    /// <summary>True when a valid invitation is required to complete signup.</summary>
    bool RequireInvitationForSignup { get; }
    bool EmailEnabled { get; }
    bool SupportsAdminTools { get; }
    /// <summary>Maximum households allowed in this deployment. 0 means unlimited.</summary>
    int MaxHouseholdsPerDeployment { get; }
}
