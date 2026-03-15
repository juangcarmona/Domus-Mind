using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks.Events;
using DomusMind.Domain.Tasks.ValueObjects;

namespace DomusMind.Domain.Tasks;

public sealed class HouseholdTask : AggregateRoot<TaskId>
{
    public FamilyId FamilyId { get; private set; }
    public TaskTitle Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime? DueDate { get; private set; }
    public TaskStatus Status { get; private set; }
    public MemberId? AssigneeId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private HouseholdTask(
        TaskId id,
        FamilyId familyId,
        TaskTitle title,
        string? description,
        DateTime? dueDate,
        DateTime createdAtUtc)
        : base(id)
    {
        FamilyId = familyId;
        Title = title;
        Description = description;
        DueDate = dueDate;
        Status = TaskStatus.Pending;
        CreatedAtUtc = createdAtUtc;
    }

    public static HouseholdTask Create(
        TaskId id,
        FamilyId familyId,
        TaskTitle title,
        string? description,
        DateTime? dueDate,
        DateTime createdAtUtc)
    {
        var task = new HouseholdTask(id, familyId, title, description, dueDate, createdAtUtc);
        task.RaiseDomainEvent(new TaskCreated(
            Guid.NewGuid(), id.Value, familyId.Value, title.Value, dueDate, createdAtUtc));
        return task;
    }

    public void Assign(MemberId assigneeId)
    {
        if (Status == TaskStatus.Completed)
            throw new InvalidOperationException("Cannot assign a completed task.");

        if (Status == TaskStatus.Cancelled)
            throw new InvalidOperationException("Cannot assign a cancelled task.");

        AssigneeId = assigneeId;
        RaiseDomainEvent(new TaskAssigned(
            Guid.NewGuid(), Id.Value, assigneeId.Value, DateTime.UtcNow));
    }

    public void Complete()
    {
        if (Status == TaskStatus.Completed)
            throw new InvalidOperationException("Task is already completed.");

        if (Status == TaskStatus.Cancelled)
            throw new InvalidOperationException("Cannot complete a cancelled task.");

        Status = TaskStatus.Completed;
        RaiseDomainEvent(new TaskCompleted(
            Guid.NewGuid(), Id.Value, DateTime.UtcNow));
    }

    public void Cancel()
    {
        if (Status == TaskStatus.Cancelled)
            throw new InvalidOperationException("Task is already cancelled.");

        if (Status == TaskStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed task.");

        Status = TaskStatus.Cancelled;
        RaiseDomainEvent(new TaskCancelled(
            Guid.NewGuid(), Id.Value, DateTime.UtcNow));
    }

    public void Reassign(MemberId newAssigneeId)
    {
        if (Status == TaskStatus.Completed)
            throw new InvalidOperationException("Cannot reassign a completed task.");

        if (Status == TaskStatus.Cancelled)
            throw new InvalidOperationException("Cannot reassign a cancelled task.");

        var previousAssigneeId = AssigneeId;
        AssigneeId = newAssigneeId;
        RaiseDomainEvent(new TaskReassigned(
            Guid.NewGuid(), Id.Value, FamilyId.Value, previousAssigneeId?.Value, newAssigneeId.Value, DateTime.UtcNow));
    }

    public void Reschedule(DateTime? newDueDate)
    {
        if (Status == TaskStatus.Completed)
            throw new InvalidOperationException("Cannot reschedule a completed task.");

        if (Status == TaskStatus.Cancelled)
            throw new InvalidOperationException("Cannot reschedule a cancelled task.");

        DueDate = newDueDate;
        RaiseDomainEvent(new TaskRescheduled(
            Guid.NewGuid(), Id.Value, newDueDate, DateTime.UtcNow));
    }

#pragma warning disable CS8618
    // EF Core parameterless constructor
    private HouseholdTask() : base(default) { }
#pragma warning restore CS8618
}
