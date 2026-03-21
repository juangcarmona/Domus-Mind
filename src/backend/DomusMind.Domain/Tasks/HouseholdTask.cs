using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family;
using DomusMind.Domain.Shared;
using DomusMind.Domain.Tasks.Enums;
using DomusMind.Domain.Tasks.Events;
using DomusMind.Domain.Tasks.ValueObjects;

namespace DomusMind.Domain.Tasks;

public sealed class HouseholdTask : AggregateRoot<TaskId>
{
    public FamilyId FamilyId { get; private set; }
    public TaskTitle Title { get; private set; }
    public string? Description { get; private set; }
    public TaskSchedule Schedule { get; private set; }
    public HexColor Color { get; private set; }
    public HouseholdTaskStatus Status { get; private set; }
    public MemberId? AssigneeId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private HouseholdTask(
        TaskId id,
        FamilyId familyId,
        TaskTitle title,
        string? description,
        TaskSchedule schedule,
        HexColor color,
        DateTime createdAtUtc)
        : base(id)
    {
        FamilyId = familyId;
        Title = title;
        Description = description;
        Schedule = schedule;
        Color = color;
        Status = HouseholdTaskStatus.Pending;
        CreatedAtUtc = createdAtUtc;
    }

    public static HouseholdTask Create(
        TaskId id,
        FamilyId familyId,
        TaskTitle title,
        string? description,
        TaskSchedule schedule,
        HexColor color,
        DateTime createdAtUtc)
    {
        var task = new HouseholdTask(id, familyId, title, description, schedule, color, createdAtUtc);
        task.RaiseDomainEvent(new TaskCreated(
            Guid.NewGuid(), id.Value, familyId.Value, title.Value, schedule, createdAtUtc));
        return task;
    }

    public void Repaint(HexColor newColor)
    {
        if (Status == HouseholdTaskStatus.Completed)
            throw new InvalidOperationException("Cannot repaint a completed task.");

        if (Status == HouseholdTaskStatus.Cancelled)
            throw new InvalidOperationException("Cannot repaint a cancelled task.");

        Color = newColor;
    }

    public void Assign(MemberId assigneeId)
    {
        if (Status == HouseholdTaskStatus.Completed)
            throw new InvalidOperationException("Cannot assign a completed task.");

        if (Status == HouseholdTaskStatus.Cancelled)
            throw new InvalidOperationException("Cannot assign a cancelled task.");

        AssigneeId = assigneeId;
        RaiseDomainEvent(new TaskAssigned(
            Guid.NewGuid(), Id.Value, assigneeId.Value, DateTime.UtcNow));
    }

    public void Complete()
    {
        if (Status == HouseholdTaskStatus.Completed)
            throw new InvalidOperationException("Task is already completed.");

        if (Status == HouseholdTaskStatus.Cancelled)
            throw new InvalidOperationException("Cannot complete a cancelled task.");

        Status = HouseholdTaskStatus.Completed;
        RaiseDomainEvent(new TaskCompleted(
            Guid.NewGuid(), Id.Value, DateTime.UtcNow));
    }

    public void Cancel()
    {
        if (Status == HouseholdTaskStatus.Cancelled)
            throw new InvalidOperationException("Task is already cancelled.");

        if (Status == HouseholdTaskStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed task.");

        Status = HouseholdTaskStatus.Cancelled;
        RaiseDomainEvent(new TaskCancelled(
            Guid.NewGuid(), Id.Value, DateTime.UtcNow));
    }

    public void Reassign(MemberId newAssigneeId)
    {
        if (Status == HouseholdTaskStatus.Completed)
            throw new InvalidOperationException("Cannot reassign a completed task.");

        if (Status == HouseholdTaskStatus.Cancelled)
            throw new InvalidOperationException("Cannot reassign a cancelled task.");

        var previousAssigneeId = AssigneeId;
        AssigneeId = newAssigneeId;
        RaiseDomainEvent(new TaskReassigned(
            Guid.NewGuid(), Id.Value, FamilyId.Value, previousAssigneeId?.Value, newAssigneeId.Value, DateTime.UtcNow));
    }

    public void Reschedule(TaskSchedule newSchedule)
    {
        if (Status == HouseholdTaskStatus.Completed)
            throw new InvalidOperationException("Cannot reschedule a completed task.");

        if (Status == HouseholdTaskStatus.Cancelled)
            throw new InvalidOperationException("Cannot reschedule a cancelled task.");

        Schedule = newSchedule;
        RaiseDomainEvent(new TaskRescheduled(
            Guid.NewGuid(), Id.Value, newSchedule, DateTime.UtcNow));
    }

    public void Rename(TaskTitle newTitle)
    {
        if (Status == HouseholdTaskStatus.Completed)
            throw new InvalidOperationException("Cannot rename a completed task.");

        if (Status == HouseholdTaskStatus.Cancelled)
            throw new InvalidOperationException("Cannot rename a cancelled task.");

        Title = newTitle;
    }

#pragma warning disable CS8618
    // EF Core parameterless constructor
    private HouseholdTask() : base(default) { }
#pragma warning restore CS8618
}
