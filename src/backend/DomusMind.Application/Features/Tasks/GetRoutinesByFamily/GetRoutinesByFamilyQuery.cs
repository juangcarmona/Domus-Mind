using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Tasks;

namespace DomusMind.Application.Features.Tasks.GetRoutinesByFamily;

public sealed record GetRoutinesByFamilyQuery(
    Guid FamilyId,
    Guid RequestedByUserId)
    : IQuery<RoutineListResponse>;
