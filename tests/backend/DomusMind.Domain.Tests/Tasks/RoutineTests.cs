using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.Events;
using DomusMind.Domain.Tasks.ValueObjects;
using FluentAssertions;

namespace DomusMind.Domain.Tests.Tasks;

public sealed class RoutineTests
{
    private static Routine BuildRoutine(
        string name = "Evening Cleanup",
        string cadence = "Daily at 20:00")
        => Routine.Create(
            RoutineId.New(),
            FamilyId.New(),
            RoutineName.Create(name),
            cadence,
            DateTime.UtcNow);

    // --- RoutineName value object ---

    [Fact]
    public void RoutineName_WithValidValue_TrimsWhitespace()
    {
        var name = RoutineName.Create("  Weekly Review  ");
        name.Value.Should().Be("Weekly Review");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RoutineName_WithEmptyValue_Throws(string value)
    {
        var act = () => RoutineName.Create(value);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RoutineName_ExceedingMaxLength_Throws()
    {
        var act = () => RoutineName.Create(new string('x', 201));
        act.Should().Throw<ArgumentException>();
    }

    // --- Create ---

    [Fact]
    public void Create_WithValidData_SetsActiveStatus()
    {
        var routine = BuildRoutine();
        routine.Status.Should().Be(RoutineStatus.Active);
    }

    [Fact]
    public void Create_EmitsSingleRoutineCreatedEvent()
    {
        var routine = BuildRoutine();
        routine.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RoutineCreated>();
    }

    [Fact]
    public void Create_RoutineCreatedEvent_ContainsCorrectData()
    {
        var routine = BuildRoutine("Morning Stretch", "Every day at 07:00");

        var evt = (RoutineCreated)routine.DomainEvents.Single();
        evt.RoutineId.Should().Be(routine.Id.Value);
        evt.Name.Should().Be("Morning Stretch");
        evt.Cadence.Should().Be("Every day at 07:00");
    }

    [Fact]
    public void Create_WithEmptyCadence_Throws()
    {
        var act = () => Routine.Create(
            RoutineId.New(), FamilyId.New(),
            RoutineName.Create("Test"), "   ", DateTime.UtcNow);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cadence*");
    }

    // --- Update ---

    [Fact]
    public void Update_ChangesNameAndCadence()
    {
        var routine = BuildRoutine();
        routine.ClearDomainEvents();

        routine.Update(RoutineName.Create("Weekend Chores"), "Every Saturday at 10:00");

        routine.Name.Value.Should().Be("Weekend Chores");
        routine.Cadence.Should().Be("Every Saturday at 10:00");
    }

    [Fact]
    public void Update_EmitsRoutineUpdatedEvent()
    {
        var routine = BuildRoutine();
        routine.ClearDomainEvents();

        routine.Update(RoutineName.Create("New Name"), "Weekly");

        routine.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RoutineUpdated>();
    }

    [Fact]
    public void Update_WithEmptyCadence_Throws()
    {
        var routine = BuildRoutine();
        var act = () => routine.Update(RoutineName.Create("Name"), "");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cadence*");
    }

    // --- Pause ---

    [Fact]
    public void Pause_ActiveRoutine_SetsPausedStatus()
    {
        var routine = BuildRoutine();
        routine.Pause();
        routine.Status.Should().Be(RoutineStatus.Paused);
    }

    [Fact]
    public void Pause_EmitsRoutinePausedEvent()
    {
        var routine = BuildRoutine();
        routine.ClearDomainEvents();

        routine.Pause();

        routine.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RoutinePaused>();
    }

    [Fact]
    public void Pause_AlreadyPaused_Throws()
    {
        var routine = BuildRoutine();
        routine.Pause();
        var act = () => routine.Pause();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already paused*");
    }

    // --- Resume ---

    [Fact]
    public void Resume_PausedRoutine_SetsActiveStatus()
    {
        var routine = BuildRoutine();
        routine.Pause();
        routine.ClearDomainEvents();

        routine.Resume();

        routine.Status.Should().Be(RoutineStatus.Active);
    }

    [Fact]
    public void Resume_EmitsRoutineResumedEvent()
    {
        var routine = BuildRoutine();
        routine.Pause();
        routine.ClearDomainEvents();

        routine.Resume();

        routine.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RoutineResumed>();
    }

    [Fact]
    public void Resume_AlreadyActive_Throws()
    {
        var routine = BuildRoutine();
        var act = () => routine.Resume();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already active*");
    }
}
