namespace DomusMind.Contracts.Tasks;

public sealed record CancelTaskResponse(
    Guid TaskId,
    string Status);
