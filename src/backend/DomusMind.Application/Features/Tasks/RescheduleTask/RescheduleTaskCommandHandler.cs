using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Tasks;
using DomusMind.Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Tasks.RescheduleTask;

public sealed class RescheduleTaskCommandHandler
    : ICommandHandler<RescheduleTaskCommand, RescheduleTaskResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public RescheduleTaskCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<RescheduleTaskResponse> Handle(
        RescheduleTaskCommand command,
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
            task.Reschedule(command.NewDueDate);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("completed"))
        {
            throw new TasksException(TasksErrorCode.TaskAlreadyCompleted, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new TasksException(TasksErrorCode.TaskAlreadyCancelled, ex.Message);
        }

        await _eventLogWriter.WriteAsync(task.DomainEvents, cancellationToken);
        task.ClearDomainEvents();

        return new RescheduleTaskResponse(command.TaskId, command.NewDueDate, task.Status.ToString());
    }
}
