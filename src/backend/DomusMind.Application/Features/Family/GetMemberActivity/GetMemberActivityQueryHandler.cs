using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Family;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Family.GetMemberActivity;

public sealed class GetMemberActivityQueryHandler
    : IQueryHandler<GetMemberActivityQuery, MemberActivityResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetMemberActivityQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<MemberActivityResponse> Handle(
        GetMemberActivityQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);
        if (!canAccess)
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Access to this family is denied.");

        var familyId = FamilyId.From(query.FamilyId);
        var memberId = MemberId.From(query.MemberId);

        // Confirm the member belongs to this family
        var family = await _dbContext.Set<Domain.Family.Family>()
            .AsNoTracking()
            .Include(f => f.Members)
            .SingleOrDefaultAsync(f => f.Id == familyId, cancellationToken);

        if (family is null)
            throw new FamilyException(FamilyErrorCode.FamilyNotFound, "Family not found.");

        var member = family.Members.SingleOrDefault(m => m.Id == memberId);
        if (member is null)
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Member not found in this family.");

        // Calendar events where member is a participant (load all, filter in-memory)
        var allFamilyEvents = await _dbContext.Set<CalendarEvent>()
            .AsNoTracking()
            .Where(e => e.FamilyId == familyId)
            .OrderBy(e => e.StartTime)
            .ToListAsync(cancellationToken);

        var memberEvents = allFamilyEvents
            .Where(e => e.ParticipantIds.Any(p => p.Value == memberId.Value))
            .Select(e => new MemberCalendarActivity(
                e.Id.Value, e.Title.Value, e.StartTime, e.EndTime, e.Status.ToString()))
            .ToList();

        // Tasks assigned to the member
        var memberTasks = await _dbContext.Set<HouseholdTask>()
            .AsNoTracking()
            .Where(t => t.FamilyId == familyId && t.AssigneeId == memberId)
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);

        var taskActivities = memberTasks
            .Select(t => new MemberTaskActivity(
                t.Id.Value, t.Title.Value, t.DueDate, t.Status.ToString()))
            .ToList();

        // Responsibility domains where member is primary or secondary owner (load all, filter in-memory)
        var allDomains = await _dbContext.Set<ResponsibilityDomain>()
            .AsNoTracking()
            .Where(d => d.FamilyId == familyId)
            .ToListAsync(cancellationToken);

        var responsibilityActivities = allDomains
            .Where(d => (d.PrimaryOwnerId.HasValue && d.PrimaryOwnerId.Value.Value == memberId.Value)
                     || d.SecondaryOwnerIds.Any(s => s.Value == memberId.Value))
            .Select(d =>
            {
                var role = d.PrimaryOwnerId.HasValue && d.PrimaryOwnerId.Value.Value == memberId.Value
                    ? "PrimaryOwner"
                    : "SecondaryOwner";
                return new MemberResponsibilityActivity(d.Id.Value, d.Name.Value, role);
            })
            .OrderBy(r => r.DomainName)
            .ToList();

        return new MemberActivityResponse(
            member.Id.Value,
            member.Name.Value,
            memberEvents,
            taskActivities,
            responsibilityActivities);
    }
}
