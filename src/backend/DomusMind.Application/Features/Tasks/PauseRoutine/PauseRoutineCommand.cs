using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Tasks;

namespace DomusMind.Application.Features.Tasks.PauseRoutine;

public sealed record PauseRoutineCommand(
    Guid RoutineId,
    Guid RequestedByUserId)
    : ICommand<PauseRoutineResponse>;
