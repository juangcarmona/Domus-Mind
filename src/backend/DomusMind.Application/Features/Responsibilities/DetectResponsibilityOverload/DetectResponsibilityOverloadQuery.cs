using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Responsibilities;

namespace DomusMind.Application.Features.Responsibilities.DetectResponsibilityOverload;

public sealed record DetectResponsibilityOverloadQuery(
    Guid FamilyId,
    int Threshold,
    Guid RequestedByUserId) : IQuery<ResponsibilityOverloadResponse>;
