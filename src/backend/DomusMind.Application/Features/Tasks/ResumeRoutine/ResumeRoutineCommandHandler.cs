using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Tasks;
using DomusMind.Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Tasks.ResumeRoutine;

public sealed class ResumeRoutineCommandHandler
    : ICommandHandler<ResumeRoutineCommand, ResumeRoutineResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public ResumeRoutineCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<ResumeRoutineResponse> Handle(
        ResumeRoutineCommand command,
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

        try
        {
            routine.Resume();
        }
        catch (InvalidOperationException ex)
        {
            throw new TasksException(TasksErrorCode.RoutineAlreadyActive, ex.Message);
        }

        await _eventLogWriter.WriteAsync(routine.DomainEvents, cancellationToken);
        routine.ClearDomainEvents();

        return new ResumeRoutineResponse(command.RoutineId, routine.Status.ToString());
    }
}
