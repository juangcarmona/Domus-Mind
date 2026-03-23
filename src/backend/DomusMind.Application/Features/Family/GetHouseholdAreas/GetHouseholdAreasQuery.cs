using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Family;

namespace DomusMind.Application.Features.Family.GetHouseholdAreas;

public sealed record GetHouseholdAreasQuery(
    Guid FamilyId,
    Guid RequestedByUserId) : IQuery<HouseholdAreasResponse>;
