namespace DomusMind.Contracts.Tasks;

public sealed record CompleteTaskResponse(
    Guid TaskId,
    string Status);
