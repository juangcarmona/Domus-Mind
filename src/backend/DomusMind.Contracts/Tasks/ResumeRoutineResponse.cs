namespace DomusMind.Contracts.Tasks;

public sealed record ResumeRoutineResponse(
    Guid RoutineId,
    string Status);
