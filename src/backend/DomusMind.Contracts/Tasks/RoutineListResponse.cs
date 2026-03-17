namespace DomusMind.Contracts.Tasks;

public sealed record RoutineListItem(
    Guid RoutineId,
    Guid FamilyId,
    string Name,
    string Scope,
    string Kind,
    string Color,
    string Frequency,
    IReadOnlyCollection<DayOfWeek> DaysOfWeek,
    IReadOnlyCollection<int> DaysOfMonth,
    int? MonthOfYear,
    TimeOnly? Time,
    IReadOnlyCollection<Guid> TargetMemberIds,
    string Status,
    DateTime CreatedAtUtc);
    
public sealed record RoutineListResponse(
    IReadOnlyCollection<RoutineListItem> Routines);
