using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Tasks;

namespace DomusMind.Application.Features.Tasks.CompleteTask;

public sealed record CompleteTaskCommand(
    Guid TaskId,
    Guid RequestedByUserId)
    : ICommand<CompleteTaskResponse>;
