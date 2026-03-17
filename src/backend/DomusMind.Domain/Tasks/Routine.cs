using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks.Enums;
using DomusMind.Domain.Tasks.Events;
using DomusMind.Domain.Tasks.ValueObjects;

namespace DomusMind.Domain.Tasks;

public sealed class Routine : AggregateRoot<RoutineId>
{
    private readonly List<RoutineTargetMember> _targetMembers = [];

    public FamilyId FamilyId { get; private set; }
    public RoutineName Name { get; private set; }
    public RoutineScope Scope { get; private set; }
    public RoutineKind Kind { get; private set; }
    public RoutineColor Color { get; private set; }
    public RoutineSchedule Schedule { get; private set; }
    public RoutineStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<MemberId> TargetMemberIds =>
        _targetMembers.Select(x => x.MemberId).ToArray();

    private Routine(
        RoutineId id,
        FamilyId familyId,
        RoutineName name,
        RoutineScope scope,
        RoutineKind kind,
        RoutineColor color,
        RoutineSchedule schedule,
        IEnumerable<MemberId> targetMembers,
        DateTime createdAtUtc)
        : base(id)
    {
        FamilyId = familyId;
        Name = name;
        Scope = scope;
        Kind = kind;
        Color = color;
        Schedule = schedule;
        Status = RoutineStatus.Active;
        CreatedAtUtc = createdAtUtc;

        ReplaceTargetMembers(targetMembers);
    }

    public static Routine Create(
        RoutineId id,
        FamilyId familyId,
        RoutineName name,
        RoutineScope scope,
        RoutineKind kind,
        RoutineColor color,
        RoutineSchedule schedule,
        IEnumerable<MemberId>? targetMembers,
        DateTime createdAtUtc)
    {
        var members = targetMembers?.Distinct().ToList() ?? [];

        ValidateScope(scope, members);

        var routine = new Routine(
            id,
            familyId,
            name,
            scope,
            kind,
            color,
            schedule,
            members,
            createdAtUtc);

        routine.RaiseDomainEvent(new RoutineCreated(
            Guid.NewGuid(),
            id.Value,
            familyId.Value,
            name.Value,
            scope.ToString(),
            kind.ToString(),
            color.Value,
            createdAtUtc));

        return routine;
    }

    public void Update(
        RoutineName newName,
        RoutineScope newScope,
        RoutineKind newKind,
        RoutineColor newColor,
        RoutineSchedule newSchedule,
        IEnumerable<MemberId>? targetMembers)
    {
        var members = targetMembers?.Distinct().ToList() ?? [];

        ValidateScope(newScope, members);

        Name = newName;
        Scope = newScope;
        Kind = newKind;
        Color = newColor;
        Schedule = newSchedule;

        ReplaceTargetMembers(members);

        RaiseDomainEvent(new RoutineUpdated(
            Guid.NewGuid(),
            Id.Value,
            newName.Value,
            newScope.ToString(),
            newKind.ToString(),
            newColor.Value,
            DateTime.UtcNow));
    }

    public void Pause()
    {
        if (Status == RoutineStatus.Paused)
            throw new InvalidOperationException("Routine is already paused.");

        Status = RoutineStatus.Paused;

        RaiseDomainEvent(new RoutinePaused(
            Guid.NewGuid(),
            Id.Value,
            DateTime.UtcNow));
    }

    public void Resume()
    {
        if (Status == RoutineStatus.Active)
            throw new InvalidOperationException("Routine is already active.");

        Status = RoutineStatus.Active;

        RaiseDomainEvent(new RoutineResumed(
            Guid.NewGuid(),
            Id.Value,
            DateTime.UtcNow));
    }

    public bool AppliesTo(MemberId memberId)
        => Scope == RoutineScope.Household || _targetMembers.Any(x => x.MemberId == memberId);

    private void ReplaceTargetMembers(IEnumerable<MemberId> members)
    {
        _targetMembers.Clear();
        _targetMembers.AddRange(members.Select(RoutineTargetMember.Create));
    }

    private static void ValidateScope(RoutineScope scope, IReadOnlyCollection<MemberId> members)
    {
        if (scope == RoutineScope.Members && members.Count == 0)
            throw new InvalidOperationException("Member-scoped routine must target at least one member.");
    }

#pragma warning disable CS8618
    private Routine() : base(default) { }
#pragma warning restore CS8618
}