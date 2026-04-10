using DomusMind.Application.Abstractions.Messaging;

namespace DomusMind.Application.Features.Lists.ArchiveList;

public sealed record ArchiveListCommand(
    Guid ListId,
    Guid RequestedByUserId) : ICommand<bool>;
