using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Tasks;

namespace DomusMind.Application.Features.Tasks.UpdateRoutine;

public sealed record UpdateRoutineCommand(
    Guid RoutineId,
    string Name,
    string Cadence,
    Guid RequestedByUserId)
    : ICommand<UpdateRoutineResponse>;
