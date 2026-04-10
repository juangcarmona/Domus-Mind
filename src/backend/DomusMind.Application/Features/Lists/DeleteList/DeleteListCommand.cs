using DomusMind.Application.Abstractions.Messaging;

namespace DomusMind.Application.Features.Lists.DeleteList;

public sealed record DeleteListCommand(
    Guid ListId,
    Guid RequestedByUserId) : ICommand<bool>;
