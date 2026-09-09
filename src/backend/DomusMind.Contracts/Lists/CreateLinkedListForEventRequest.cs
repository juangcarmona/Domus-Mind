namespace DomusMind.Contracts.Lists;

public sealed record CreateLinkedListForEventRequest(
    Guid FamilyId,
    string? Name);
