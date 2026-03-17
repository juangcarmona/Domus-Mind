namespace DomusMind.Contracts.Tasks;

public sealed record WeeklyGridRoutineItem(
    Guid RoutineId,
    string Name,
    string Kind,
    string Color,
    string Frequency,
    TimeOnly? Time,
    string Scope);