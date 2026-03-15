using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Tasks;

namespace DomusMind.Application.Features.Tasks.RescheduleTask;

public sealed record RescheduleTaskCommand(
    Guid TaskId,
    DateTime? NewDueDate,
    Guid RequestedByUserId)
    : ICommand<RescheduleTaskResponse>;
