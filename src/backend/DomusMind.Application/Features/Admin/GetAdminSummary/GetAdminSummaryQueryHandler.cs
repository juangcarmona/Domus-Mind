using DomusMind.Application.Abstractions.Admin;
using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Platform;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Abstractions.System;
using DomusMind.Contracts.Admin;
using Microsoft.EntityFrameworkCore;
using DomusMind.Application.Abstractions.Persistence;
using FamilyAggregate = DomusMind.Domain.Family.Family;

namespace DomusMind.Application.Features.Admin.GetAdminSummary;

public sealed class GetAdminSummaryQueryHandler : IQueryHandler<GetAdminSummaryQuery, AdminSummaryResponse>
{
    private readonly IDomusMindDbContext _db;
    private readonly IAuthUserRepository _users;
    private readonly IOperatorInvitationRepository _invitations;
    private readonly IDeploymentModeContext _deployment;
    private readonly ISystemInitializationState _initState;

    public GetAdminSummaryQueryHandler(
        IDomusMindDbContext db,
        IAuthUserRepository users,
        IOperatorInvitationRepository invitations,
        IDeploymentModeContext deployment,
        ISystemInitializationState initState)
    {
        _db = db;
        _users = users;
        _invitations = invitations;
        _deployment = deployment;
        _initState = initState;
    }

    public async Task<AdminSummaryResponse> Handle(
        GetAdminSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var householdCount = await _db.Set<FamilyAggregate>().CountAsync(cancellationToken);
        var userCount = await _users.CountAsync(cancellationToken);
        var pendingCount = await _invitations.CountPendingAsync(cancellationToken);
        var isInitialized = await _initState.IsInitializedAsync(cancellationToken);

        return new AdminSummaryResponse(
            DeploymentMode: _deployment.Mode.ToString(),
            HouseholdCount: householdCount,
            UserCount: userCount,
            PendingInvitationCount: pendingCount,
            IsSystemInitialized: isInitialized,
            AllowHouseholdCreation: _deployment.CanCreateHousehold,
            InvitationsEnabled: _deployment.InvitationsEnabled,
            RequireInvitationForSignup: _deployment.RequireInvitationForSignup,
            AdminToolsEnabled: _deployment.SupportsAdminTools,
            MaxHouseholdsPerDeployment: _deployment.MaxHouseholdsPerDeployment);
    }
}
