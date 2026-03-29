using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Platform;

namespace DomusMind.Application.Features.Platform.GetCurrentDeploymentMode;

public sealed record GetCurrentDeploymentModeQuery : IQuery<DeploymentModeResponse>;
