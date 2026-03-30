using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Platform;
using DomusMind.Application.Abstractions.System;
using DomusMind.Contracts.Setup;

namespace DomusMind.Application.Features.Setup.GetSetupStatus;

public sealed class GetSetupStatusQueryHandler : IQueryHandler<GetSetupStatusQuery, SetupStatusResponse>
{
    private readonly IDeploymentModeContext _deployment;
    private readonly ISystemInitializationState _state;

    public GetSetupStatusQueryHandler(
        IDeploymentModeContext deployment,
        ISystemInitializationState state)
    {
        _deployment = deployment;
        _state = state;
    }

    public async Task<SetupStatusResponse> Handle(GetSetupStatusQuery query, CancellationToken cancellationToken)
    {
        // SingleInstance has no setup phase — the system is always considered initialized.
        if (_deployment.Mode == DeploymentMode.SingleInstance)
            return new SetupStatusResponse(IsInitialized: true);

        var initialized = await _state.IsInitializedAsync(cancellationToken);
        return new SetupStatusResponse(initialized);
    }
}
