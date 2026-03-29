using DomusMind.Application.Abstractions.Platform;

namespace DomusMind.Infrastructure.Platform;

/// <summary>
/// Strongly typed deployment configuration resolved from appsettings / environment variables.
/// Validated at startup; invalid combinations fail fast.
/// </summary>
public sealed class DeploymentSettings
{
    public const string SectionName = "Deployment";

    public string Mode { get; init; } = "SingleInstance";

    // Household provisioning policy
    public bool AllowHouseholdCreation { get; init; } = true;

    // Invitations
    public bool InvitationsEnabled { get; init; } = false;
    public bool RequireInvitationForSignup { get; init; } = false;

    // Email
    public bool EmailEnabled { get; init; } = false;

    // Admin tools
    public bool AdminToolsEnabled { get; init; } = false;

    public DeploymentMode ResolvedMode =>
        Enum.TryParse<DeploymentMode>(Mode, ignoreCase: true, out var m) ? m : DeploymentMode.SingleInstance;

    public void Validate()
    {
        if (!Enum.TryParse<DeploymentMode>(Mode, ignoreCase: true, out _))
            throw new InvalidOperationException(
                $"Invalid Deployment:Mode value '{Mode}'. Allowed values: SingleInstance, CloudHosted.");

        if (ResolvedMode == DeploymentMode.SingleInstance && InvitationsEnabled)
            throw new InvalidOperationException(
                "Invalid deployment configuration: InvitationsEnabled is not supported in SingleInstance mode.");

        if (ResolvedMode == DeploymentMode.SingleInstance && RequireInvitationForSignup)
            throw new InvalidOperationException(
                "Invalid deployment configuration: RequireInvitationForSignup is not supported in SingleInstance mode.");

        if (RequireInvitationForSignup && !InvitationsEnabled)
            throw new InvalidOperationException(
                "Invalid deployment configuration: RequireInvitationForSignup requires InvitationsEnabled = true.");
    }
}
