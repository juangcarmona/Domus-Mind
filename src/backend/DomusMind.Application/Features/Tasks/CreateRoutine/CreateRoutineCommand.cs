using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Tasks;

namespace DomusMind.Application.Features.Tasks.CreateRoutine;

public sealed record CreateRoutineCommand(
    string Name,
    Guid FamilyId,
    string Scope,
    string Kind,
    string Color,
    string Frequency,
    IReadOnlyCollection<DayOfWeek> DaysOfWeek,
    IReadOnlyCollection<int> DaysOfMonth,
    int? MonthOfYear,
    TimeOnly? Time,
    IReadOnlyCollection<Guid> TargetMemberIds,
    Guid RequestedByUserId)
    : ICommand<CreateRoutineResponse>;