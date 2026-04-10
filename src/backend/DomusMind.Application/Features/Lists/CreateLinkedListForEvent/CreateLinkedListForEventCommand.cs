using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Lists;

namespace DomusMind.Application.Features.Lists.CreateLinkedListForEvent;

public sealed record CreateLinkedListForEventCommand(
    Guid CalendarEventId,
    Guid FamilyId,
    string? Name,
    Guid RequestedByUserId) : ICommand<CreateListResponse>;
