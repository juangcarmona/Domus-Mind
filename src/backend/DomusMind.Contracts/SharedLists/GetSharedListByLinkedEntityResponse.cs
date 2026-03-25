namespace DomusMind.Contracts.SharedLists;

/// <summary>
/// Returns null when no linked list is found (HTTP 404 at controller level).
/// </summary>
public sealed record GetSharedListByLinkedEntityResponse(
    Guid ListId,
    string Name,
    string Kind,
    int ItemCount,
    int UncheckedCount);
