namespace DomusMind.Contracts.SharedLists;

public sealed record LinkSharedListRequest(
    string LinkedEntityType,
    Guid LinkedEntityId);
