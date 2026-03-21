using DomusMind.Domain.Family;
using DomusMind.Domain.Shared;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.Enums;
using DomusMind.Domain.Tasks.Events;
using DomusMind.Domain.Tasks.ValueObjects;
using FluentAssertions;

namespace DomusMind.Domain.Tests.Tasks;

public sealed class HouseholdTaskTests
{
    private static HouseholdTask BuildTask(
        DateOnly? dueDate = null,
        string title = "Clean Kitchen")
        => HouseholdTask.Create(
            TaskId.New(),
            FamilyId.New(),
            TaskTitle.Create(title),
            null,
            dueDate.HasValue ? TaskSchedule.WithDueDate(dueDate.Value) : TaskSchedule.NoSchedule(),
            HexColor.From("#3B82F6"),
            DateTime.UtcNow);

    // --- TaskTitle value object ---

    [Fact]
    public void TaskTitle_WithValidValue_ReturnsTitle()
    {
        var title = TaskTitle.Create("  Buy groceries  ");
        title.Value.Should().Be("Buy groceries");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void TaskTitle_WithEmptyValue_Throws(string value)
    {
        var act = () => TaskTitle.Create(value);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TaskTitle_ExceedingMaxLength_Throws()
    {
        var act = () => TaskTitle.Create(new string('x', 201));
        act.Should().Throw<ArgumentException>();
    }

    // --- Create ---

    [Fact]
    public void Create_WithValidData_SetsPendingStatus()
    {
        var task = BuildTask();
        task.Status.Should().Be(HouseholdTaskStatus.Pending);
    }

    [Fact]
    public void Create_EmitsSingleTaskCreatedEvent()
    {
        var task = BuildTask();
        task.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TaskCreated>();
    }

    [Fact]
    public void Create_TaskCreatedEvent_ContainsCorrectData()
    {
        var dueDate = new DateOnly(2026, 4, 10);
        var task = BuildTask(dueDate, "Feed the cat");

        var evt = (TaskCreated)task.DomainEvents.Single();
        evt.TaskId.Should().Be(task.Id.Value);
        evt.FamilyId.Should().Be(task.FamilyId.Value);
        evt.Title.Should().Be("Feed the cat");
        evt.Schedule.Date.Should().Be(dueDate);
    }

    // --- Assign ---

    [Fact]
    public void Assign_PendingTask_SetsAssigneeId()
    {
        var task = BuildTask();
        task.ClearDomainEvents();
        var memberId = MemberId.New();

        task.Assign(memberId);

        task.AssigneeId.Should().Be(memberId);
    }

    [Fact]
    public void Assign_EmitsTaskAssignedEvent()
    {
        var task = BuildTask();
        task.ClearDomainEvents();
        var memberId = MemberId.New();

        task.Assign(memberId);

        task.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TaskAssigned>();
    }

    [Fact]
    public void Assign_TaskAssignedEvent_ContainsMemberId()
    {
        var task = BuildTask();
        task.ClearDomainEvents();
        var memberId = MemberId.New();

        task.Assign(memberId);

        var evt = (TaskAssigned)task.DomainEvents.Single();
        evt.AssigneeId.Should().Be(memberId.Value);
    }

    [Fact]
    public void Assign_CompletedTask_Throws()
    {
        var task = BuildTask();
        task.Complete();
        var act = () => task.Assign(MemberId.New());
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*completed*");
    }

    [Fact]
    public void Assign_CancelledTask_Throws()
    {
        var task = BuildTask();
        task.Cancel();
        var act = () => task.Assign(MemberId.New());
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cancelled*");
    }

    // --- Complete ---

    [Fact]
    public void Complete_PendingTask_SetsCompletedStatus()
    {
        var task = BuildTask();
        task.Complete();
        task.Status.Should().Be(HouseholdTaskStatus.Completed);
    }

    [Fact]
    public void Complete_EmitsTaskCompletedEvent()
    {
        var task = BuildTask();
        task.ClearDomainEvents();

        task.Complete();

        task.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TaskCompleted>();
    }

    [Fact]
    public void Complete_AlreadyCompleted_Throws()
    {
        var task = BuildTask();
        task.Complete();
        var act = () => task.Complete();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already completed*");
    }

    [Fact]
    public void Complete_CancelledTask_Throws()
    {
        var task = BuildTask();
        task.Cancel();
        var act = () => task.Complete();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cancelled*");
    }

    // --- Cancel ---

    [Fact]
    public void Cancel_PendingTask_SetsCancelledStatus()
    {
        var task = BuildTask();
        task.Cancel();
        task.Status.Should().Be(HouseholdTaskStatus.Cancelled);
    }

    [Fact]
    public void Cancel_EmitsTaskCancelledEvent()
    {
        var task = BuildTask();
        task.ClearDomainEvents();

        task.Cancel();

        task.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TaskCancelled>();
    }

    [Fact]
    public void Cancel_AlreadyCancelled_Throws()
    {
        var task = BuildTask();
        task.Cancel();
        var act = () => task.Cancel();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already cancelled*");
    }

    [Fact]
    public void Cancel_CompletedTask_Throws()
    {
        var task = BuildTask();
        task.Complete();
        var act = () => task.Cancel();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*completed*");
    }

    // --- Reschedule ---

    [Fact]
    public void Reschedule_PendingTask_UpdatesDueDate()
    {
        var task = BuildTask();
        var newDue = new DateOnly(2026, 5, 1);

        task.Reschedule(TaskSchedule.WithDueDate(newDue));

        task.Schedule.Date.Should().Be(newDue);
    }

    [Fact]
    public void Reschedule_ToNoSchedule_ClearsDueDate()
    {
        var task = BuildTask(new DateOnly(2026, 4, 5));

        task.Reschedule(TaskSchedule.NoSchedule());

        task.Schedule.HasSchedule.Should().BeFalse();
    }

    [Fact]
    public void Reschedule_EmitsTaskRescheduledEvent()
    {
        var task = BuildTask();
        task.ClearDomainEvents();
        var newDue = new DateOnly(2026, 5, 10);

        task.Reschedule(TaskSchedule.WithDueDate(newDue));

        task.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TaskRescheduled>();
    }

    [Fact]
    public void Reschedule_CompletedTask_Throws()
    {
        var task = BuildTask();
        task.Complete();
        var act = () => task.Reschedule(TaskSchedule.WithDueDate(new DateOnly(2026, 5, 1)));
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*completed*");
    }

    [Fact]
    public void Reschedule_CancelledTask_Throws()
    {
        var task = BuildTask();
        task.Cancel();
        var act = () => task.Reschedule(TaskSchedule.WithDueDate(new DateOnly(2026, 5, 1)));
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cancelled*");
    }
}
