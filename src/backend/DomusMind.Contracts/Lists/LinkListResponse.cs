namespace DomusMind.Contracts.Lists;

public sealed record LinkListResponse(
    Guid ListId,
    string LinkedEntityType,
    Guid LinkedEntityId);
