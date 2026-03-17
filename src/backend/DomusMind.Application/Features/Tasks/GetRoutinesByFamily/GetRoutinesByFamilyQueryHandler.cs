using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Tasks;
using DomusMind.Contracts.Tasks;
using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Tasks.GetRoutinesByFamily;

public sealed class GetRoutinesByFamilyQueryHandler
    : IQueryHandler<GetRoutinesByFamilyQuery, RoutineListResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetRoutinesByFamilyQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<RoutineListResponse> Handle(
        GetRoutinesByFamilyQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);

        if (!canAccess)
            throw new TasksException(TasksErrorCode.AccessDenied, "Access to this family is denied.");

        var familyId = FamilyId.From(query.FamilyId);

        var routines = await _dbContext.Set<Routine>()
            .AsNoTracking()
            .Include("_targetMembers")
            .Where(r => r.FamilyId == familyId)
            .OrderBy(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var items = routines
            .Select(r => new RoutineListItem(
                r.Id.Value,
                r.FamilyId.Value,
                r.Name.Value,
                r.Scope.ToString(),
                r.Kind.ToString(),
                r.Color.Value,
                r.Schedule.Frequency.ToString(),
                r.Schedule.DaysOfWeek.ToArray(),
                r.Schedule.DaysOfMonth.ToArray(),
                r.Schedule.MonthOfYear,
                r.Schedule.Time,
                r.TargetMemberIds.Select(x => x.Value).ToArray(),
                r.Status.ToString(),
                r.CreatedAtUtc))
            .ToList();

        return new RoutineListResponse(items);
    }
}