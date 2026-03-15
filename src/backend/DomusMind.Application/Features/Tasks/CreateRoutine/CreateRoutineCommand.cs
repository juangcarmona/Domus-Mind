using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Tasks;

namespace DomusMind.Application.Features.Tasks.CreateRoutine;

public sealed record CreateRoutineCommand(
    string Name,
    Guid FamilyId,
    string Cadence,
    Guid RequestedByUserId)
    : ICommand<CreateRoutineResponse>;
