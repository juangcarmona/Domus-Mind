using DomusMind.Application.Abstractions.Platform;
using DomusMind.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Infrastructure.Platform;

/// <summary>
/// Evaluates whether a new household may be created based on the active deployment mode and current state.
/// </summary>
public sealed class HouseholdProvisioningPolicy : IHouseholdProvisioningPolicy
{
    private readonly IDeploymentModeContext _context;
    private readonly DomusMindDbContext _db;

    public HouseholdProvisioningPolicy(IDeploymentModeContext context, DomusMindDbContext db)
    {
        _context = context;
        _db = db;
    }

    public async Task<ProvisioningPolicyResult> EvaluateAsync(CancellationToken cancellationToken)
    {
        if (!_context.CanCreateHousehold)
            return ProvisioningPolicyResult.DenyCreationDisabled();

        if (_context.InvitationsEnabled)
            return ProvisioningPolicyResult.DenyInvitationRequired();

        if (_context.Mode == DeploymentMode.SingleInstance)
        {
            var count = await _db.Families.AsNoTracking().CountAsync(cancellationToken);
            if (count >= 1)
                return ProvisioningPolicyResult.DenySingleInstanceBound();
        }

        return ProvisioningPolicyResult.Permit();
    }
}
