using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Tasks;
using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.Enums;
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
            command.RequestedByUserId,
            routine.FamilyId.Value,
            cancellationToken);

        if (!canAccess)
            throw new TasksException(TasksErrorCode.AccessDenied, "Access to this family is denied.");

        try
        {
            var newName = RoutineName.Create(command.Name);
            var newScope = ParseScope(command.Scope);
            var newKind = ParseKind(command.Kind);
            var newColor = RoutineColor.From(command.Color);
            var newSchedule = BuildSchedule(command);
            var targetMembers = (command.TargetMemberIds ?? Array.Empty<Guid>())
                .Distinct()
                .Select(MemberId.From)
                .ToArray();

            routine.Update(
                newName,
                newScope,
                newKind,
                newColor,
                newSchedule,
                targetMembers);

            await _eventLogWriter.WriteAsync(routine.DomainEvents, cancellationToken);
            routine.ClearDomainEvents();

            return new UpdateRoutineResponse(
                routine.Id.Value,
                routine.Name.Value,
                routine.Scope.ToString(),
                routine.Kind.ToString(),
                routine.Color.Value,
                routine.Schedule.Frequency.ToString(),
                routine.Schedule.DaysOfWeek.ToArray(),
                routine.Schedule.DaysOfMonth.ToArray(),
                routine.Schedule.MonthOfYear,
                routine.Schedule.Time,
                routine.TargetMemberIds.Select(x => x.Value).ToArray(),
                routine.Status.ToString());
        }
        catch (ArgumentException ex)
        {
            throw new TasksException(TasksErrorCode.InvalidInput, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new TasksException(TasksErrorCode.InvalidInput, ex.Message);
        }
    }

    private static RoutineScope ParseScope(string raw)
    {
        if (!Enum.TryParse<RoutineScope>(raw, true, out var value))
            throw new InvalidOperationException("Routine scope is invalid.");

        return value;
    }

    private static RoutineKind ParseKind(string raw)
    {
        if (!Enum.TryParse<RoutineKind>(raw, true, out var value))
            throw new InvalidOperationException("Routine kind is invalid.");

        return value;
    }

    private static RoutineSchedule BuildSchedule(UpdateRoutineCommand command)
    {
        if (!Enum.TryParse<RoutineFrequency>(command.Frequency, true, out var frequency))
            throw new InvalidOperationException("Routine frequency is invalid.");

        return frequency switch
        {
            RoutineFrequency.Weekly => RoutineSchedule.Weekly(
                command.DaysOfWeek ?? Array.Empty<DayOfWeek>(),
                command.Time),

            RoutineFrequency.Monthly => RoutineSchedule.Monthly(
                command.DaysOfMonth ?? Array.Empty<int>(),
                command.Time),

            RoutineFrequency.Yearly => RoutineSchedule.Yearly(
                command.MonthOfYear ?? throw new InvalidOperationException("Yearly routine requires month of year."),
                command.DaysOfMonth ?? Array.Empty<int>(),
                command.Time),

            _ => throw new InvalidOperationException("Unsupported routine frequency.")
        };
    }
}