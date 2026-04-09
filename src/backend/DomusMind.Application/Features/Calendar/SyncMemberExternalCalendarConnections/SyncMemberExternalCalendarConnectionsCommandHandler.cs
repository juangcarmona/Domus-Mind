using DomusMind.Application.Abstractions.Integrations.Calendar;
using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar.ExternalConnections;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DomusMind.Application.Features.Calendar.SyncMemberExternalCalendarConnections;

public sealed class SyncMemberExternalCalendarConnectionsCommandHandler
    : ICommandHandler<SyncMemberExternalCalendarConnectionsCommand, SyncMemberExternalCalendarConnectionsResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;
    private readonly IExternalCalendarSyncLeaseService _leaseService;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly ILogger<SyncMemberExternalCalendarConnectionsCommandHandler> _logger;

    public SyncMemberExternalCalendarConnectionsCommandHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService,
        IExternalCalendarSyncLeaseService leaseService,
        ICommandDispatcher commandDispatcher,
        ILogger<SyncMemberExternalCalendarConnectionsCommandHandler> logger)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
        _leaseService = leaseService;
        _commandDispatcher = commandDispatcher;
        _logger = logger;
    }

    public async Task<SyncMemberExternalCalendarConnectionsResponse> Handle(
        SyncMemberExternalCalendarConnectionsCommand command,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);

        if (!canAccess)
            throw new CalendarException(CalendarErrorCode.AccessDenied, "Access to this family is denied.");

        var memberId = MemberId.From(command.MemberId);

        var connections = await _dbContext
            .Set<ExternalCalendarConnection>()
            .AsNoTracking()
            .Where(c => c.MemberId == memberId &&
                        c.Status != ExternalCalendarConnectionStatus.Disconnected)
            .Select(c => c.Id.Value)
            .ToListAsync(cancellationToken);

        int accepted = 0, skipped = 0;

        foreach (var connectionId in connections)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var syncCommand = new SyncExternalCalendarConnection.SyncExternalCalendarConnectionCommand(
                    command.FamilyId, command.MemberId, connectionId, command.Reason, command.RequestedByUserId);

                await _commandDispatcher.Dispatch(syncCommand, cancellationToken);
                accepted++;
            }
            catch (CalendarException ex) when (ex.Code == CalendarErrorCode.ConnectionSyncInProgress)
            {
                skipped++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Sync failed for connection {ConnectionId}", connectionId);
                skipped++;
            }
        }

        return new SyncMemberExternalCalendarConnectionsResponse(
            command.MemberId,
            connections.Count,
            accepted,
            skipped,
            "completed");
    }
}
