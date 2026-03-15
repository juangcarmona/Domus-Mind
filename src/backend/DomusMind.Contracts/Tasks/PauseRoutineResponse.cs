namespace DomusMind.Contracts.Tasks;

public sealed record PauseRoutineResponse(
    Guid RoutineId,
    string Status);
