using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Tasks;

namespace DomusMind.Application.Features.Tasks.CreateTask;

public sealed record CreateTaskCommand(
    string Title,
    Guid FamilyId,
    string? Description,
    string? DueDate,
    string? DueTime,
    string? Color,
    Guid RequestedByUserId)
    : ICommand<CreateTaskResponse>;
