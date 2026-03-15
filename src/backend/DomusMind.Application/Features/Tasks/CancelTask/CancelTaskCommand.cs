using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Tasks;

namespace DomusMind.Application.Features.Tasks.CancelTask;

public sealed record CancelTaskCommand(
    Guid TaskId,
    Guid RequestedByUserId)
    : ICommand<CancelTaskResponse>;
