using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Family;

namespace DomusMind.Application.Features.Family.GetMyFamily;

public sealed record GetMyFamilyQuery(Guid RequestedByUserId)
    : IQuery<FamilyResponse>;
