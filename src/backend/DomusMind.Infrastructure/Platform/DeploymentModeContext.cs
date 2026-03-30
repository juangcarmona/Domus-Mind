using DomusMind.Application.Abstractions.Platform;

namespace DomusMind.Infrastructure.Platform;

public sealed class DeploymentModeContext : IDeploymentModeContext
{
    private readonly DeploymentSettings _settings;

    public DeploymentModeContext(DeploymentSettings settings)
    {
        _settings = settings;
    }

    public DeploymentMode Mode => _settings.ResolvedMode;
    public bool CanCreateHousehold => _settings.AllowHouseholdCreation;
    public bool InvitationsEnabled => _settings.InvitationsEnabled;
    public bool RequireInvitationForSignup => _settings.RequireInvitationForSignup;
    public bool EmailEnabled => _settings.EmailEnabled;
    public bool SupportsAdminTools => _settings.AdminToolsEnabled;
    public int MaxHouseholdsPerDeployment => _settings.MaxHouseholdsPerDeployment;
}
