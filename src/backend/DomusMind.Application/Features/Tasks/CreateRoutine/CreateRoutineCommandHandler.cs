using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Tasks;
using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.ValueObjects;

namespace DomusMind.Application.Features.Tasks.CreateRoutine;

public sealed class CreateRoutineCommandHandler
    : ICommandHandler<CreateRoutineCommand, CreateRoutineResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public CreateRoutineCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<CreateRoutineResponse> Handle(
        CreateRoutineCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new TasksException(TasksErrorCode.InvalidInput, "Routine name is required.");

        if (string.IsNullOrWhiteSpace(command.Cadence))
            throw new TasksException(TasksErrorCode.InvalidInput, "Cadence is required.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);

        if (!canAccess)
            throw new TasksException(TasksErrorCode.AccessDenied, "Access to this family is denied.");

        var id = RoutineId.New();
        var familyId = FamilyId.From(command.FamilyId);
        var name = RoutineName.Create(command.Name);
        var now = DateTime.UtcNow;

        Routine routine;
        try
        {
            routine = Routine.Create(id, familyId, name, command.Cadence, now);
        }
        catch (InvalidOperationException ex)
        {
            throw new TasksException(TasksErrorCode.InvalidInput, ex.Message);
        }

        _dbContext.Set<Routine>().Add(routine);

        await _eventLogWriter.WriteAsync(routine.DomainEvents, cancellationToken);
        routine.ClearDomainEvents();

        return new CreateRoutineResponse(
            id.Value, familyId.Value, name.Value,
            routine.Cadence, routine.Status.ToString(), now);
    }
}
