namespace DomusMind.Contracts.SharedLists;

public sealed record CreateLinkedSharedListForEventRequest(
    Guid FamilyId,
    string? Name);
