using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.System;
using DomusMind.Contracts.Setup;

namespace DomusMind.Application.Features.Setup.GetSetupStatus;

public sealed class GetSetupStatusQueryHandler : IQueryHandler<GetSetupStatusQuery, SetupStatusResponse>
{
    private readonly ISystemInitializationState _state;

    public GetSetupStatusQueryHandler(ISystemInitializationState state)
    {
        _state = state;
    }

    public async Task<SetupStatusResponse> Handle(GetSetupStatusQuery query, CancellationToken cancellationToken)
    {
        var initialized = await _state.IsInitializedAsync(cancellationToken);
        return new SetupStatusResponse(initialized);
    }
}
