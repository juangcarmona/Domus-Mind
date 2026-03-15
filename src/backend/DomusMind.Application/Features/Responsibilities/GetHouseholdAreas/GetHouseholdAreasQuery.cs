using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Responsibilities;

namespace DomusMind.Application.Features.Responsibilities.GetHouseholdAreas;

public sealed record GetHouseholdAreasQuery(
    Guid FamilyId,
    Guid RequestedByUserId) : IQuery<HouseholdAreasResponse>;
