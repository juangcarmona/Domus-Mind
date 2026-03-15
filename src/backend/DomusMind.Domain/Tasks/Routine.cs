using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks.Events;
using DomusMind.Domain.Tasks.ValueObjects;

namespace DomusMind.Domain.Tasks;

public sealed class Routine : AggregateRoot<RoutineId>
{
    public FamilyId FamilyId { get; private set; }
    public RoutineName Name { get; private set; }
    public string Cadence { get; private set; }
    public RoutineStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Routine(
        RoutineId id,
        FamilyId familyId,
        RoutineName name,
        string cadence,
        DateTime createdAtUtc)
        : base(id)
    {
        FamilyId = familyId;
        Name = name;
        Cadence = cadence;
        Status = RoutineStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    public static Routine Create(
        RoutineId id,
        FamilyId familyId,
        RoutineName name,
        string cadence,
        DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(cadence))
            throw new InvalidOperationException("Cadence cannot be empty.");

        var routine = new Routine(id, familyId, name, cadence.Trim(), createdAtUtc);
        routine.RaiseDomainEvent(new RoutineCreated(
            Guid.NewGuid(), id.Value, familyId.Value, name.Value, cadence.Trim(), createdAtUtc));
        return routine;
    }

    public void Update(RoutineName newName, string newCadence)
    {
        if (string.IsNullOrWhiteSpace(newCadence))
            throw new InvalidOperationException("Cadence cannot be empty.");

        Name = newName;
        Cadence = newCadence.Trim();

        RaiseDomainEvent(new RoutineUpdated(
            Guid.NewGuid(), Id.Value, newName.Value, newCadence.Trim(), DateTime.UtcNow));
    }

    public void Pause()
    {
        if (Status == RoutineStatus.Paused)
            throw new InvalidOperationException("Routine is already paused.");

        Status = RoutineStatus.Paused;
        RaiseDomainEvent(new RoutinePaused(
            Guid.NewGuid(), Id.Value, DateTime.UtcNow));
    }

    public void Resume()
    {
        if (Status == RoutineStatus.Active)
            throw new InvalidOperationException("Routine is already active.");

        Status = RoutineStatus.Active;
        RaiseDomainEvent(new RoutineResumed(
            Guid.NewGuid(), Id.Value, DateTime.UtcNow));
    }

#pragma warning disable CS8618
    // EF Core parameterless constructor
    private Routine() : base(default) { }
#pragma warning restore CS8618
}
