using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Responsibilities;

namespace DomusMind.Application.Features.Responsibilities.GetResponsibilityVisibility;

public sealed record GetResponsibilityVisibilityQuery(
    Guid FamilyId,
    Guid RequestedByUserId) : IQuery<ResponsibilityVisibilityResponse>;
