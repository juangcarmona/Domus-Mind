using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Family;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Family.GetFamilyMembers;

public sealed class GetFamilyMembersQueryHandler
    : IQueryHandler<GetFamilyMembersQuery, IReadOnlyCollection<MemberDirectoryItemResponse>>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;
    private readonly IAuthUserRepository _authUserRepository;

    public GetFamilyMembersQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService,
        IAuthUserRepository authUserRepository)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
        _authUserRepository = authUserRepository;
    }

    public async Task<IReadOnlyCollection<MemberDirectoryItemResponse>> Handle(
        GetFamilyMembersQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);

        if (!canAccess)
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Access to this family is denied.");

        var family = await _dbContext.Set<Domain.Family.Family>()
            .AsNoTracking()
            .Include(f => f.Members)
            .SingleOrDefaultAsync(f => f.Id == FamilyId.From(query.FamilyId), cancellationToken);

        if (family is null)
            throw new FamilyException(FamilyErrorCode.FamilyNotFound, "Family was not found.");

        var requestingMember = family.Members.FirstOrDefault(m => m.AuthUserId == query.RequestedByUserId);
        var isRequestingManager = requestingMember?.IsManager ?? false;

        var linkedUserIds = family.Members
            .Where(m => m.AuthUserId.HasValue)
            .Select(m => m.AuthUserId!.Value)
            .ToList();

        var authStatus = await _authUserRepository.GetStatusByIdsAsync(linkedUserIds, cancellationToken);

        return family.Members
            .OrderBy(m => m.Role.Value switch { "Adult" => 0, "Child" => 1, "Pet" => 2, _ => 3 })
            .ThenByDescending(m => m.IsManager)
            .ThenBy(m => AccessSortOrder(m, authStatus))
            .ThenBy(m => m.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select(m => BuildDirectoryItem(m, family.Id.Value, query.RequestedByUserId, isRequestingManager, authStatus))
            .ToList()
            .AsReadOnly();
    }

    // Active before inactive: managers first ensured above, then sort non-active last
    private static int AccessSortOrder(Domain.Family.FamilyMember m, IReadOnlyDictionary<Guid, AuthUserStatusProjection> authStatus)
    {
        if (!m.AuthUserId.HasValue) return 2;
        if (!authStatus.TryGetValue(m.AuthUserId.Value, out var p)) return 2;
        if (p.IsDisabled) return 3;
        return 1; // has account and enabled
    }

    internal static MemberDirectoryItemResponse BuildDirectoryItem(
        Domain.Family.FamilyMember m,
        Guid familyId,
        Guid requestedByUserId,
        bool isRequestingManager,
        IReadOnlyDictionary<Guid, AuthUserStatusProjection> authStatus)
    {
        MemberAccessStatus status;
        string? linkedEmail = null;

        if (!m.AuthUserId.HasValue)
        {
            status = MemberAccessStatus.NoAccess;
        }
        else if (authStatus.TryGetValue(m.AuthUserId.Value, out var projection))
        {
            linkedEmail = projection.Email;
            if (projection.IsDisabled)
                status = MemberAccessStatus.Disabled;
            else if (projection.MustChangePassword && projection.LastLoginAtUtc is null)
                status = MemberAccessStatus.InvitedOrProvisioned;
            else if (projection.MustChangePassword)
                status = MemberAccessStatus.PasswordResetRequired;
            else
                status = MemberAccessStatus.Active;
        }
        else
        {
            status = MemberAccessStatus.NoAccess;
        }

        var isCurrentUser = m.AuthUserId == requestedByUserId;
        var hasAccount = m.AuthUserId.HasValue;
        var isPet = m.Role.Value == "Pet";

        // Managers can grant access to non-pet members that don't already have an account
        var canGrantAccess = isRequestingManager && !hasAccount && !isPet;

        // Managers can edit anyone; members can edit themselves
        var canEdit = isRequestingManager || isCurrentUser;

        return new MemberDirectoryItemResponse(
            m.Id.Value,
            familyId,
            m.Name.Value,
            m.PreferredName,
            m.Role.Value,
            m.IsManager,
            m.BirthDate,
            m.JoinedAtUtc,
            m.AuthUserId,
            status,
            linkedEmail,
            isCurrentUser,
            hasAccount,
            canGrantAccess,
            canEdit,
            m.DisplayName.Length > 0 ? m.DisplayName[0].ToString().ToUpperInvariant() : "?");
    }
}

