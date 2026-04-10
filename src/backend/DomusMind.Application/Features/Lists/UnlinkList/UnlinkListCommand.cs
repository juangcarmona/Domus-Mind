using DomusMind.Application.Abstractions.Messaging;

namespace DomusMind.Application.Features.Lists.UnlinkList;

public sealed record UnlinkListCommand(
    Guid ListId,
    Guid RequestedByUserId) : ICommand<bool>;
