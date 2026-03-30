using DomusMind.Application.Abstractions.Platform;
using DomusMind.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DomusMind.Infrastructure.Platform;

/// <summary>
/// Evaluates whether a new household may be created based on the active deployment mode and current state.
/// </summary>
public sealed class HouseholdProvisioningPolicy : IHouseholdProvisioningPolicy
{
    private readonly IDeploymentModeContext _context;
    private readonly DomusMindDbContext _db;
    private readonly ILogger<HouseholdProvisioningPolicy> _logger;

    public HouseholdProvisioningPolicy(
        IDeploymentModeContext context,
        DomusMindDbContext db,
        ILogger<HouseholdProvisioningPolicy> logger)
    {
        _context = context;
        _db = db;
        _logger = logger;
    }

    public async Task<ProvisioningPolicyResult> EvaluateAsync(CancellationToken cancellationToken)
    {
        if (!_context.CanCreateHousehold)
            return Log(ProvisioningPolicyResult.DenyCreationDisabled());

        if (_context.RequireInvitationForSignup)
            return Log(ProvisioningPolicyResult.DenyInvitationRequired());

        if (_context.Mode == DeploymentMode.SingleInstance)
        {
            var count = await _db.Families.AsNoTracking().CountAsync(cancellationToken);
            if (count >= 1)
                return Log(ProvisioningPolicyResult.DenySingleInstanceBound());
        }
        else if (_context.MaxHouseholdsPerDeployment > 0)
        {
            var count = await _db.Families.AsNoTracking().CountAsync(cancellationToken);
            if (count >= _context.MaxHouseholdsPerDeployment)
                return Log(ProvisioningPolicyResult.DenyMaxHouseholdsReached());
        }

        return Log(ProvisioningPolicyResult.Permit());
    }

    private ProvisioningPolicyResult Log(ProvisioningPolicyResult result)
    {
        _logger.LogInformation(
            "Household creation {Decision}: {ReasonCode} | DeploymentMode={DeploymentMode} AllowHouseholdCreation={AllowHouseholdCreation} InvitationsEnabled={InvitationsEnabled} RequireInvitationForSignup={RequireInvitationForSignup} MaxHouseholdsPerDeployment={MaxHouseholdsPerDeployment}",
            result.Allowed ? "allowed" : "denied",
            result.ReasonCode,
            _context.Mode,
            _context.CanCreateHousehold,
            _context.InvitationsEnabled,
            _context.RequireInvitationForSignup,
            _context.MaxHouseholdsPerDeployment);
        return result;
    }
}
