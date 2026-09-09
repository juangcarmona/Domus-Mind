using DomusMind.Application.Abstractions.Messaging;

namespace DomusMind.Application.Features.Lists.RestoreList;

public sealed record RestoreListCommand(
    Guid ListId,
    Guid RequestedByUserId) : ICommand<bool>;
