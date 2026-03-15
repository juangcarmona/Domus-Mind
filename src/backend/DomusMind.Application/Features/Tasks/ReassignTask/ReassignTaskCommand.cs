using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Tasks;

namespace DomusMind.Application.Features.Tasks.ReassignTask;

public sealed record ReassignTaskCommand(
    Guid TaskId,
    Guid NewAssigneeId,
    Guid RequestedByUserId) : ICommand<ReassignTaskResponse>;
