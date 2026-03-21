using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Temporal;
using DomusMind.Contracts.Tasks;
using DomusMind.Domain.Family;
using DomusMind.Domain.Shared;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.ValueObjects;

namespace DomusMind.Application.Features.Tasks.CreateTask;

public sealed class CreateTaskCommandHandler
    : ICommandHandler<CreateTaskCommand, CreateTaskResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public CreateTaskCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<CreateTaskResponse> Handle(
        CreateTaskCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            throw new TasksException(TasksErrorCode.InvalidInput, "Task title is required.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);

        if (!canAccess)
            throw new TasksException(TasksErrorCode.AccessDenied, "Access to this family is denied.");

        TaskSchedule schedule;
        try
        {
            schedule = TemporalParser.ParseTaskSchedule(command.DueDate, command.DueTime);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            throw new TasksException(TasksErrorCode.InvalidInput, ex.Message);
        }

        HexColor taskColor;
        try
        {
            taskColor = HexColor.From(command.Color ?? "#3B82F6");
        }
        catch (InvalidOperationException ex)
        {
            throw new TasksException(TasksErrorCode.InvalidInput, ex.Message);
        }

        var id = TaskId.New();
        var familyId = FamilyId.From(command.FamilyId);
        var title = TaskTitle.Create(command.Title);
        var now = DateTime.UtcNow;

        var task = HouseholdTask.Create(id, familyId, title, command.Description, schedule, taskColor, now);
        _dbContext.Set<HouseholdTask>().Add(task);

        await _eventLogWriter.WriteAsync(task.DomainEvents, cancellationToken);
        task.ClearDomainEvents();

        var (dueDate, dueTime) = TemporalParser.FormatTaskSchedule(schedule);

        return new CreateTaskResponse(
            id.Value, familyId.Value, title.Value,
            command.Description, dueDate, dueTime,
            task.Status.ToString(), taskColor.Value, now);
    }
}
