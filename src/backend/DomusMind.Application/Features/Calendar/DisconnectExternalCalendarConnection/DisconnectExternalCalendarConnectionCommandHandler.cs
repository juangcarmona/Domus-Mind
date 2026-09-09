using DomusMind.Application.Abstractions.Integrations.Calendar;
using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Domain.Calendar.ExternalConnections;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DomusMind.Application.Features.Calendar.DisconnectExternalCalendarConnection;

public sealed class DisconnectExternalCalendarConnectionCommandHandler
    : ICommandHandler<DisconnectExternalCalendarConnectionCommand, bool>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;
    private readonly IExternalCalendarAuthService _authService;
    private readonly ILogger<DisconnectExternalCalendarConnectionCommandHandler> _logger;

    public DisconnectExternalCalendarConnectionCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService,
        IExternalCalendarAuthService authService,
        ILogger<DisconnectExternalCalendarConnectionCommandHandler> logger)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
        _authService = authService;
        _logger = logger;
    }

    public async Task<bool> Handle(
        DisconnectExternalCalendarConnectionCommand command,
        CancellationToken cancellationToken)
    {
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
            return true;

        var now = DateTime.UtcNow;
        connection.Disconnect(now);

        // Remove imported entries for all feeds
        var feedIds = connection.Feeds.Select(f => f.Id).ToList();
        if (feedIds.Count > 0)
        {
            var entries = await _dbContext
                .Set<ExternalCalendarEntry>()
                .Where(e => feedIds.Contains(e.FeedId))
                .ToListAsync(cancellationToken);

            _dbContext.Set<ExternalCalendarEntry>().RemoveRange(entries);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Revoke stored auth material
        try
        {
            await _authService.RevokeAsync(command.ConnectionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to revoke auth material for connection {ConnectionId}", command.ConnectionId);
        }

        await _eventLogWriter.WriteAsync(connection.DomainEvents, cancellationToken);
        connection.ClearDomainEvents();

        return true;
    }
}
