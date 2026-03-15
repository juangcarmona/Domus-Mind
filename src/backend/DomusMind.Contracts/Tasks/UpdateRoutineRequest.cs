namespace DomusMind.Contracts.Tasks;

public sealed record UpdateRoutineRequest(
    string Name,
    string Cadence);
