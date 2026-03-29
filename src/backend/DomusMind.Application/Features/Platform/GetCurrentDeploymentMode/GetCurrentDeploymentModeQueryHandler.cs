using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Platform;
using DomusMind.Contracts.Platform;

namespace DomusMind.Application.Features.Platform.GetCurrentDeploymentMode;

public sealed class GetCurrentDeploymentModeQueryHandler
    : IQueryHandler<GetCurrentDeploymentModeQuery, DeploymentModeResponse>
{
    private readonly IDeploymentModeContext _context;
    private readonly IHouseholdProvisioningPolicy _policy;

    public GetCurrentDeploymentModeQueryHandler(
        IDeploymentModeContext context,
        IHouseholdProvisioningPolicy policy)
    {
        _context = context;
        _policy = policy;
    }

    public async Task<DeploymentModeResponse> Handle(
        GetCurrentDeploymentModeQuery query,
        CancellationToken cancellationToken)
    {
        var policyResult = await _policy.EvaluateAsync(cancellationToken);

        return new DeploymentModeResponse(
            DeploymentMode: _context.Mode.ToString(),
            CanCreateHousehold: policyResult.Allowed,
            RequiresInvitation: _context.RequireInvitationForSignup,
            SupportsEmail: _context.EmailEnabled,
            SupportsAdminTools: _context.SupportsAdminTools);
    }
}
