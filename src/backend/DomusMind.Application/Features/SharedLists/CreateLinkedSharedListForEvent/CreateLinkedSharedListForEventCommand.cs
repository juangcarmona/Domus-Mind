using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.SharedLists;

namespace DomusMind.Application.Features.SharedLists.CreateLinkedSharedListForEvent;

public sealed record CreateLinkedSharedListForEventCommand(
    Guid CalendarEventId,
    Guid FamilyId,
    string? Name,
    Guid RequestedByUserId) : ICommand<CreateSharedListResponse>;
