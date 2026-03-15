using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Tasks;
using DomusMind.Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Tasks.CancelTask;

public sealed class CancelTaskCommandHandler
    : ICommandHandler<CancelTaskCommand, CancelTaskResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public CancelTaskCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<CancelTaskResponse> Handle(
        CancelTaskCommand command,
        CancellationToken cancellationToken)
    {
        var task = await _dbContext
            .Set<HouseholdTask>()
            .SingleOrDefaultAsync(
                t => t.Id == TaskId.From(command.TaskId),
                cancellationToken);

        if (task is null)
            throw new TasksException(TasksErrorCode.TaskNotFound, "Task was not found.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, task.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new TasksException(TasksErrorCode.AccessDenied, "Access to this family is denied.");

        try
        {
            task.Cancel();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already cancelled"))
        {
            throw new TasksException(TasksErrorCode.TaskAlreadyCancelled, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new TasksException(TasksErrorCode.TaskAlreadyCompleted, ex.Message);
        }

        await _eventLogWriter.WriteAsync(task.DomainEvents, cancellationToken);
        task.ClearDomainEvents();

        return new CancelTaskResponse(command.TaskId, task.Status.ToString());
    }
}
