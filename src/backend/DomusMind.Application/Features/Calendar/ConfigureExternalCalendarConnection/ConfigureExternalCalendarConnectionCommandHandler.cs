using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar.ExternalConnections;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DomusMind.Application.Features.Calendar.ConfigureExternalCalendarConnection;

public sealed class ConfigureExternalCalendarConnectionCommandHandler
    : ICommandHandler<ConfigureExternalCalendarConnectionCommand, ConfigureExternalCalendarConnectionResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;
    private readonly ILogger<ConfigureExternalCalendarConnectionCommandHandler> _logger;

    public ConfigureExternalCalendarConnectionCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService,
        ILogger<ConfigureExternalCalendarConnectionCommandHandler> logger)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public async Task<ConfigureExternalCalendarConnectionResponse> Handle(
        ConfigureExternalCalendarConnectionCommand command,
        CancellationToken cancellationToken)
    {
        var allowedDays = SyncHorizon.AllowedForwardDays;
        if (!allowedDays.Contains(command.ForwardHorizonDays))
            throw new CalendarException(CalendarErrorCode.InvalidInput,
                $"ForwardHorizonDays must be one of {string.Join(", ", allowedDays)}.");

        if (command.ScheduledRefreshIntervalMinutes < 15)
            throw new CalendarException(CalendarErrorCode.InvalidInput,
                "ScheduledRefreshIntervalMinutes must be at least 15.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);

        if (!canAccess)
            throw new CalendarException(CalendarErrorCode.AccessDenied, "Access to this family is denied.");

        var connectionId = ExternalCalendarConnectionId.From(command.ConnectionId);
        var memberId = MemberId.From(command.MemberId);

        var connection = await _dbContext
            .Set<ExternalCalendarConnection>()
            .Include(c => c.Feeds)
            .FirstOrDefaultAsync(c => c.Id == connectionId && c.MemberId == memberId, cancellationToken);

        if (connection is null)
            throw new CalendarException(CalendarErrorCode.ConnectionNotFound, "Connection not found.");

        if (connection.Status == ExternalCalendarConnectionStatus.Disconnected)
            throw new CalendarException(CalendarErrorCode.ConnectionNotFound, "Connection has been disconnected.");

        var now = DateTime.UtcNow;

        // -----------------------------------------------------------------------
        // Feed merge — always operate on EF-tracked entities in place.
        //
        // Using the aggregate's _feeds collection to add new entities would bypass
        // EF's change tracker and cause DbUpdateConcurrencyException (0-row UPDATE)
        // when the saved rows do not match the tracked snapshots.
        //
        // Rules:
        //   • Existing feed (matched by ProviderCalendarId): update in place.
        //   • New calendar in selections not yet in DB: add via DbSet.
        //   • Feed in DB not present in incoming selections: deselect it.
        // -----------------------------------------------------------------------

        var existingByCalendarId = connection.Feeds
            .ToDictionary(f => f.ProviderCalendarId, StringComparer.OrdinalIgnoreCase);

        var incomingIndex = command.SelectedCalendars
            .ToDictionary(s => s.CalendarId, s => s, StringComparer.OrdinalIgnoreCase);

        int added = 0, updated = 0, deselected = 0;

        // Update in-place or add new.
        foreach (var (calendarId, calendarName, isSelected, colorHex) in command.SelectedCalendars)
        {
            if (existingByCalendarId.TryGetValue(calendarId, out var existing))
            {
                existing.UpdateSelection(calendarName, isSelected, now, colorHex);
                updated++;
            }
            else
            {
                var newFeed = ExternalCalendarFeed.Create(connection.Id, calendarId, calendarName, false, isSelected, now, colorHex);
                _dbContext.Set<ExternalCalendarFeed>().Add(newFeed);
                added++;
            }
        }

        // Deselect feeds not mentioned in the incoming selections.
        foreach (var (calId, feed) in existingByCalendarId)
        {
            if (!incomingIndex.ContainsKey(calId) && feed.IsSelected)
            {
                feed.UpdateSelection(feed.CalendarName, false, now);
                deselected++;
            }
        }

        int selectedFeedCount = command.SelectedCalendars.Count(s => s.IsSelected);

        _logger.LogInformation(
            "Configuring connection {ConnectionId}: {Added} feed(s) added, {Updated} updated, " +
            "{Deselected} deselected, {Selected} total selected",
            command.ConnectionId, added, updated, deselected, selectedFeedCount);

        // Apply aggregate-level settings (horizon, refresh schedule, domain event).
        // Feed delta invalidation on horizon change operates only on _feeds, which
        // contains the already-loaded EF entities — consistent with tracked updates above.
        connection.Configure(
            command.ForwardHorizonDays,
            command.ScheduledRefreshEnabled,
            command.ScheduledRefreshIntervalMinutes,
            selectedFeedCount,
            now);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _eventLogWriter.WriteAsync(connection.DomainEvents, cancellationToken);
        connection.ClearDomainEvents();

        return new ConfigureExternalCalendarConnectionResponse(
            connection.Id.Value,
            selectedFeedCount,
            connection.Horizon.ForwardHorizonDays,
            connection.ScheduledRefreshEnabled,
            connection.ScheduledRefreshIntervalMinutes,
            ExternalCalendarConnectionStatusNames.ToStatusString(connection.Status));
    }
}
