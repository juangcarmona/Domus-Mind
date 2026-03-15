namespace DomusMind.Contracts.Tasks;

public sealed record UpdateRoutineResponse(
    Guid RoutineId,
    string Name,
    string Cadence,
    string Status);
