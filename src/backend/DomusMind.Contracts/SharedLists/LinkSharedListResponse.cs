namespace DomusMind.Contracts.SharedLists;

public sealed record LinkSharedListResponse(
    Guid ListId,
    string LinkedEntityType,
    Guid LinkedEntityId);
