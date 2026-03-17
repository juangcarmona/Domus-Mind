namespace DomusMind.Contracts.Tasks;

public sealed record CreateRoutineRequest(
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
    IReadOnlyCollection<Guid> TargetMemberIds);