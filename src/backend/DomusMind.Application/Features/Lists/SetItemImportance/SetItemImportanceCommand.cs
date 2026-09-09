using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Lists;

namespace DomusMind.Application.Features.Lists.SetItemImportance;

public sealed record SetItemImportanceCommand(
    Guid ListId,
    Guid ItemId,
    bool Importance,
    Guid RequestedByUserId) : ICommand<SetItemImportanceResponse>;
