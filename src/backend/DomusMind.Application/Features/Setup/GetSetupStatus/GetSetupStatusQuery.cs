using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Setup;

namespace DomusMind.Application.Features.Setup.GetSetupStatus;

public sealed record GetSetupStatusQuery : IQuery<SetupStatusResponse>;
