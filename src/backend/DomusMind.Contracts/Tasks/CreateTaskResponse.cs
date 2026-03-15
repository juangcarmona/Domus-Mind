namespace DomusMind.Contracts.Tasks;

public sealed record CreateTaskResponse(
    Guid TaskId,
    Guid FamilyId,
    string Title,
    string? Description,
    DateTime? DueDate,
    string Status,
    DateTime CreatedAtUtc);
