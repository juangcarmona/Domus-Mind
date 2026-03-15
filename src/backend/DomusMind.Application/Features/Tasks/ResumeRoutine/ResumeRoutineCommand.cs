using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Tasks;

namespace DomusMind.Application.Features.Tasks.ResumeRoutine;

public sealed record ResumeRoutineCommand(
    Guid RoutineId,
    Guid RequestedByUserId)
    : ICommand<ResumeRoutineResponse>;
