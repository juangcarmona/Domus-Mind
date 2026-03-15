using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Tasks;

namespace DomusMind.Application.Features.Tasks.AssignTask;

public sealed record AssignTaskCommand(
    Guid TaskId,
    Guid AssigneeId,
    Guid RequestedByUserId)
    : ICommand<AssignTaskResponse>;
