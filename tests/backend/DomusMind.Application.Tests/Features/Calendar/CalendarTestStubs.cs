using DomusMind.Application.Abstractions.Integrations.Calendar;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Domain.Shared;

namespace DomusMind.Application.Tests.Features.Calendar;

internal sealed class StubCalendarAuthorizationService : IFamilyAuthorizationService
{
    public bool CanAccess { get; set; } = true;

    public Task<bool> CanAccessFamilyAsync(Guid userId, Guid familyId, CancellationToken cancellationToken = default)
        => Task.FromResult(CanAccess);
}

internal sealed class StubCalendarEventLogWriter : IEventLogWriter
{
    public List<IDomainEvent> WrittenEvents { get; } = [];

    public Task WriteAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        WrittenEvents.AddRange(domainEvents);
        return Task.CompletedTask;
    }
}

internal static class CalendarTestHelpers
{
    public static Domain.Calendar.CalendarEvent MakeEvent(
        FamilyId familyId,
        string title,
        DateOnly date,
        TimeOnly? time = null,
        DateOnly? endDate = null,
        TimeOnly? endTime = null)
    {
        EventTime eventTime;
        if (time.HasValue && endDate.HasValue && endTime.HasValue)
            eventTime = EventTime.Range(date, time.Value, endDate.Value, endTime.Value);
        else if (time.HasValue)
            eventTime = EventTime.Moment(date, time.Value);
        else
            eventTime = EventTime.Day(date);

        return Domain.Calendar.CalendarEvent.Create(
            CalendarEventId.New(), familyId,
            EventTitle.Create(title), null,
            eventTime, HexColor.From("#3B82F6"), DateTime.UtcNow);
    }
}

/// <summary>Always grants a lease; does not track release calls.</summary>
internal sealed class StubExternalCalendarSyncLeaseService : IExternalCalendarSyncLeaseService
{
    public Task<Guid?> TryAcquireAsync(Guid connectionId, CancellationToken cancellationToken = default)
        => Task.FromResult<Guid?>(Guid.NewGuid());

    public Task ReleaseAsync(Guid connectionId, Guid leaseId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

/// <summary>Returns a fixed access token; all other operations are no-ops.</summary>
internal sealed class StubExternalCalendarAuthService : IExternalCalendarAuthService
{
    private readonly string? _token;

    public StubExternalCalendarAuthService(string? token = "access-token") => _token = token;

    public Task<ExternalCalendarProviderAccount> ExchangeAuthorizationCodeAsync(
        string authorizationCode, string redirectUri, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<string?> GetAccessTokenAsync(Guid connectionId, CancellationToken cancellationToken = default)
        => Task.FromResult(_token);

    public Task RevokeAsync(Guid connectionId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
