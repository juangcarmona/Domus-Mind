using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Tasks;
using DomusMind.Domain.Family;
using DomusMind.Domain.Shared;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.Enums;
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

        if (string.IsNullOrWhiteSpace(command.Scope))
            throw new TasksException(TasksErrorCode.InvalidInput, "Routine scope is required.");

        if (string.IsNullOrWhiteSpace(command.Kind))
            throw new TasksException(TasksErrorCode.InvalidInput, "Routine kind is required.");

        if (string.IsNullOrWhiteSpace(command.Color))
            throw new TasksException(TasksErrorCode.InvalidInput, "Routine color is required.");

        if (string.IsNullOrWhiteSpace(command.Frequency))
            throw new TasksException(TasksErrorCode.InvalidInput, "Routine frequency is required.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId,
            command.FamilyId,
            cancellationToken);

        if (!canAccess)
            throw new TasksException(TasksErrorCode.AccessDenied, "Access to this family is denied.");

        var id = RoutineId.New();
        var familyId = FamilyId.From(command.FamilyId);
        var name = RoutineName.Create(command.Name);
        var now = DateTime.UtcNow;

        try
        {
            var scope = ParseScope(command.Scope);
            var kind = ParseKind(command.Kind);
            var color = HexColor.From(command.Color);
            var schedule = BuildSchedule(command);
            var targetMembers = (command.TargetMemberIds ?? Array.Empty<Guid>())
                .Distinct()
                .Select(MemberId.From)
                .ToArray();

            var routine = Routine.Create(
                id,
                familyId,
                name,
                scope,
                kind,
                color,
                schedule,
                targetMembers,
                now);

            _dbContext.Set<Routine>().Add(routine);

            await _eventLogWriter.WriteAsync(routine.DomainEvents, cancellationToken);
            routine.ClearDomainEvents();

            return new CreateRoutineResponse(
                routine.Id.Value,
                routine.FamilyId.Value,
                routine.Name.Value,
                routine.Scope.ToString(),
                routine.Kind.ToString(),
                routine.Color.Value,
                schedule.Frequency.ToString(),
                schedule.DaysOfWeek.ToArray(),
                schedule.DaysOfMonth.ToArray(),
                schedule.MonthOfYear,
                schedule.Time,
                routine.TargetMemberIds.Select(x => x.Value).ToArray(),
                routine.Status.ToString(),
                routine.CreatedAtUtc);
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

    private static RoutineSchedule BuildSchedule(CreateRoutineCommand command)
    {
        if (!Enum.TryParse<RoutineFrequency>(command.Frequency, true, out var frequency))
            throw new InvalidOperationException("Routine frequency is invalid.");

        return frequency switch
        {
            RoutineFrequency.Daily => RoutineSchedule.Daily(command.Time),

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