using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Responsibilities;

namespace DomusMind.Application.Features.Responsibilities.GetResponsibilityBalance;

public sealed record GetResponsibilityBalanceQuery(
    Guid FamilyId,
    Guid RequestedByUserId) : IQuery<ResponsibilityBalanceResponse>;
