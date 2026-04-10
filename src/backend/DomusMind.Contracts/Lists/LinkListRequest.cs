namespace DomusMind.Contracts.Lists;

public sealed record LinkListRequest(
    string LinkedEntityType,
    Guid LinkedEntityId);
