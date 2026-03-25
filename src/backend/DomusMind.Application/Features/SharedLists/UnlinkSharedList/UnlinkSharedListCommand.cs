using DomusMind.Application.Abstractions.Messaging;

namespace DomusMind.Application.Features.SharedLists.UnlinkSharedList;

public sealed record UnlinkSharedListCommand(
    Guid SharedListId,
    Guid RequestedByUserId) : ICommand<bool>;
