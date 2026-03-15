using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Tasks;
using DomusMind.Domain.Family;
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

        var id = TaskId.New();
        var familyId = FamilyId.From(command.FamilyId);
        var title = TaskTitle.Create(command.Title);
        var now = DateTime.UtcNow;

        var task = HouseholdTask.Create(id, familyId, title, command.Description, command.DueDate, now);
        _dbContext.Set<HouseholdTask>().Add(task);

        await _eventLogWriter.WriteAsync(task.DomainEvents, cancellationToken);
        task.ClearDomainEvents();

        return new CreateTaskResponse(
            id.Value, familyId.Value, title.Value,
            command.Description, command.DueDate,
            task.Status.ToString(), now);
    }
}
