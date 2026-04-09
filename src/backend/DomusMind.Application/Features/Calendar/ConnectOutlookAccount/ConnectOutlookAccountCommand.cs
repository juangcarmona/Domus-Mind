using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.ConnectOutlookAccount;

public sealed record ConnectOutlookAccountCommand(
    Guid FamilyId,
    Guid MemberId,
    string AuthorizationCode,
    string RedirectUri,
    string? AccountDisplayLabel,
    Guid RequestedByUserId)
    : ICommand<ExternalCalendarConnectionDetailResponse>;
