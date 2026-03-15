namespace DomusMind.Contracts.Tasks;

public sealed record CreateRoutineRequest(
    string Name,
    Guid FamilyId,
    string Cadence);
