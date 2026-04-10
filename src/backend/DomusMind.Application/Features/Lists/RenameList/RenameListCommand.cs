using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Lists;

namespace DomusMind.Application.Features.Lists.RenameList;

public sealed record RenameListCommand(
    Guid ListId,
    string NewName,
    Guid RequestedByUserId) : ICommand<RenameListResponse>;
