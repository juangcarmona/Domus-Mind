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
    TimeOnly? EndTime,
    IReadOnlyCollection<Guid> TargetMemberIds,
    string Status,
    Guid? AreaId,
    DateTime CreatedAtUtc);
    
public sealed record RoutineListResponse(
    IReadOnlyCollection<RoutineListItem> Routines);
