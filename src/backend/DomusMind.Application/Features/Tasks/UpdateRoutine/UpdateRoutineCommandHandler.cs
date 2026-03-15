using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Tasks;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Tasks.UpdateRoutine;

public sealed class UpdateRoutineCommandHandler
    : ICommandHandler<UpdateRoutineCommand, UpdateRoutineResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public UpdateRoutineCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<UpdateRoutineResponse> Handle(
        UpdateRoutineCommand command,
        CancellationToken cancellationToken)
    {
        var routine = await _dbContext
            .Set<Routine>()
            .SingleOrDefaultAsync(
                r => r.Id == RoutineId.From(command.RoutineId),
                cancellationToken);

        if (routine is null)
            throw new TasksException(TasksErrorCode.RoutineNotFound, "Routine was not found.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, routine.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new TasksException(TasksErrorCode.AccessDenied, "Access to this family is denied.");

        RoutineName newName;
        try
        {
            newName = RoutineName.Create(command.Name);
        }
        catch (ArgumentException ex)
        {
            throw new TasksException(TasksErrorCode.InvalidInput, ex.Message);
        }

        try
        {
            routine.Update(newName, command.Cadence);
        }
        catch (InvalidOperationException ex)
        {
            throw new TasksException(TasksErrorCode.InvalidInput, ex.Message);
        }

        await _eventLogWriter.WriteAsync(routine.DomainEvents, cancellationToken);
        routine.ClearDomainEvents();

        return new UpdateRoutineResponse(
            command.RoutineId, routine.Name.Value, routine.Cadence, routine.Status.ToString());
    }
}
