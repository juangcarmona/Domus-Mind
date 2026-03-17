using DomusMind.Contracts.Family;

namespace DomusMind.Contracts.Tasks;
public sealed record WeeklyGridCell(
    DateTime Date,
    IReadOnlyCollection<WeeklyGridEventItem> Events,
    IReadOnlyCollection<WeeklyGridTaskItem> Tasks,
    IReadOnlyCollection<WeeklyGridRoutineItem> Routines);