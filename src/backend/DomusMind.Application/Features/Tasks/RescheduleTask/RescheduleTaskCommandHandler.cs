using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Temporal;
using DomusMind.Contracts.Tasks;
using DomusMind.Domain.Shared;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.ValueObjects;
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

        Domain.Tasks.ValueObjects.TaskSchedule newSchedule;
        try
        {
            newSchedule = TemporalParser.ParseTaskSchedule(command.DueDate, command.DueTime);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            throw new TasksException(TasksErrorCode.InvalidInput, ex.Message);
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(command.Title))
            {
                task.Rename(TaskTitle.Create(command.Title));
            }
            task.Reschedule(newSchedule);
            if (command.Color is not null)
            {
                task.Repaint(HexColor.From(command.Color));
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("completed"))
        {
            throw new TasksException(TasksErrorCode.TaskAlreadyCompleted, ex.Message);
        }
        catch (ArgumentException ex)
        {
            throw new TasksException(TasksErrorCode.InvalidInput, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new TasksException(TasksErrorCode.TaskAlreadyCancelled, ex.Message);
        }

        await _eventLogWriter.WriteAsync(task.DomainEvents, cancellationToken);
        task.ClearDomainEvents();

        var (dueDate, dueTime) = TemporalParser.FormatTaskSchedule(newSchedule);

        return new RescheduleTaskResponse(command.TaskId, dueDate, dueTime, task.Status.ToString(), task.Color.Value);
    }
}
