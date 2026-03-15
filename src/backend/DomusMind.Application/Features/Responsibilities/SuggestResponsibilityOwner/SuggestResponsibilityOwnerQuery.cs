using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Responsibilities;

namespace DomusMind.Application.Features.Responsibilities.SuggestResponsibilityOwner;

public sealed record SuggestResponsibilityOwnerQuery(
    Guid FamilyId,
    Guid ResponsibilityDomainId,
    Guid RequestedByUserId) : IQuery<SuggestResponsibilityOwnerResponse>;
