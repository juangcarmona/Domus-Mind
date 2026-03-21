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
