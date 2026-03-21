using DomusMind.Domain.Family;
using DomusMind.Domain.Shared;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.Events;
using DomusMind.Domain.Tasks.ValueObjects;
using FluentAssertions;

namespace DomusMind.Domain.Tests.Tasks;

public sealed class HouseholdTaskReassignTests
{
    private static HouseholdTask BuildAssignedTask()
    {
        var task = HouseholdTask.Create(
            TaskId.New(), FamilyId.New(),
            TaskTitle.Create("Clean Kitchen"), null,
            TaskSchedule.WithDueDate(new DateOnly(2026, 4, 1)),
            HexColor.From("#3B82F6"), DateTime.UtcNow);
        task.Assign(MemberId.New());
        task.ClearDomainEvents();
        return task;
    }

    private static HouseholdTask BuildUnassignedTask()
    {
        var task = HouseholdTask.Create(
            TaskId.New(), FamilyId.New(),
            TaskTitle.Create("Fix Sink"), null,
            TaskSchedule.NoSchedule(),
            HexColor.From("#3B82F6"), DateTime.UtcNow);
        task.ClearDomainEvents();
        return task;
    }

    [Fact]
    public void Reassign_ChangesAssigneeId()
    {
        var task = BuildAssignedTask();
        var newAssignee = MemberId.New();

        task.Reassign(newAssignee);

        task.AssigneeId.Should().Be(newAssignee);
    }

    [Fact]
    public void Reassign_FromUnassigned_SetsAssignee()
    {
        var task = BuildUnassignedTask();
        var newAssignee = MemberId.New();

        task.Reassign(newAssignee);

        task.AssigneeId.Should().Be(newAssignee);
    }

    [Fact]
    public void Reassign_EmitsTaskReassignedEvent()
    {
        var task = BuildAssignedTask();
        var newAssignee = MemberId.New();

        task.Reassign(newAssignee);

        task.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TaskReassigned>();
    }

    [Fact]
    public void Reassign_EventContainsPreviousAndNewAssigneeIds()
    {
        var task = BuildAssignedTask();
        var previousAssignee = task.AssigneeId!.Value;
        var newAssignee = MemberId.New();

        task.Reassign(newAssignee);

        var evt = task.DomainEvents.Single() as TaskReassigned;
        evt!.PreviousAssigneeId.Should().Be(previousAssignee.Value);
        evt.NewAssigneeId.Should().Be(newAssignee.Value);
    }

    [Fact]
    public void Reassign_CompletedTask_Throws()
    {
        var task = BuildAssignedTask();
        task.Complete();
        task.ClearDomainEvents();

        var act = () => task.Reassign(MemberId.New());
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reassign_CancelledTask_Throws()
    {
        var task = BuildAssignedTask();
        task.Cancel();
        task.ClearDomainEvents();

        var act = () => task.Reassign(MemberId.New());
        act.Should().Throw<InvalidOperationException>();
    }
}
