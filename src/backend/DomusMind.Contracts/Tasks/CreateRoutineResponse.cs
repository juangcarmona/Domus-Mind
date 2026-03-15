namespace DomusMind.Contracts.Tasks;

public sealed record CreateRoutineResponse(
    Guid RoutineId,
    Guid FamilyId,
    string Name,
    string Cadence,
    string Status,
    DateTime CreatedAtUtc);
