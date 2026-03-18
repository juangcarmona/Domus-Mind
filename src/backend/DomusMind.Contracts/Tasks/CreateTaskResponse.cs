namespace DomusMind.Contracts.Tasks;

public sealed record CreateTaskResponse(
    Guid TaskId,
    Guid FamilyId,
    string Title,
    string? Description,
    string? DueDate,
    string? DueTime,
    string Status,
    DateTime CreatedAtUtc);
