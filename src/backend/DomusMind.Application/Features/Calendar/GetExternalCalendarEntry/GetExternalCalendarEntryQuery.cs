using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.GetExternalCalendarEntry;

/// <summary>
/// Fetches a single persisted external calendar entry by its stored ID.
/// Authorization is enforced by verifying the entry belongs to a connection owned
/// by the requested member of the requested family.
/// </summary>
public sealed record GetExternalCalendarEntryQuery(
    Guid FamilyId,
    Guid MemberId,
    Guid EntryId,
    Guid RequestedByUserId)
    : IQuery<GetExternalCalendarEntryResponse>;
