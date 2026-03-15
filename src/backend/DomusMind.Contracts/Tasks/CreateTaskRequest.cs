namespace DomusMind.Contracts.Tasks;

public sealed record CreateTaskRequest(
    string Title,
    Guid FamilyId,
    string? Description,
    DateTime? DueDate);
