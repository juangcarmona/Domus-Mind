namespace DomusMind.Contracts.Lists;

/// <summary>
/// Returns null when no linked list is found (HTTP 404 at controller level).
/// </summary>
public sealed record GetListByLinkedEntityResponse(
    Guid ListId,
    string Name,
    string Kind,
    int ItemCount,
    int UncheckedCount);
