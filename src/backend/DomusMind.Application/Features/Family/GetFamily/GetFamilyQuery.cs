using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Family;

namespace DomusMind.Application.Features.Family.GetFamily;

public sealed record GetFamilyQuery(Guid FamilyId, Guid RequestedByUserId)
    : IQuery<FamilyResponse>;
