using DomusMind.Domain.Family;
using DomusMind.Domain.Shared;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.Enums;
using DomusMind.Domain.Tasks.Events;
using DomusMind.Domain.Tasks.ValueObjects;
using FluentAssertions;

namespace DomusMind.Domain.Tests.Tasks;

public sealed class RoutineTests
{
    private static Routine BuildRoutine(
        string name = "Evening Cleanup",
        RoutineScope scope = RoutineScope.Household,
        RoutineKind kind = RoutineKind.Cue,
        string color = "#7C3AED",
        RoutineSchedule? schedule = null,
        IEnumerable<MemberId>? targetMembers = null)
        => Routine.Create(
            RoutineId.New(),
            FamilyId.New(),
            RoutineName.Create(name),
            scope,
            kind,
            HexColor.From(color),
            schedule ?? RoutineSchedule.Weekly(new[] { DayOfWeek.Monday, DayOfWeek.Wednesday }),
            targetMembers,
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
    public void Create_WithHouseholdScope_HasNoTargetMembers()
    {
        var routine = BuildRoutine(scope: RoutineScope.Household);

        routine.Scope.Should().Be(RoutineScope.Household);
        routine.TargetMemberIds.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithMemberScope_SetsTargetMembers()
    {
        var memberId = MemberId.New();

        var routine = BuildRoutine(
            scope: RoutineScope.Members,
            targetMembers: new[] { memberId });

        routine.Scope.Should().Be(RoutineScope.Members);
        routine.TargetMemberIds.Should().ContainSingle().Which.Should().Be(memberId);
    }

    [Fact]
    public void Create_WithMemberScopeAndNoTargetMembers_Throws()
    {
        var act = () => BuildRoutine(
            scope: RoutineScope.Members,
            targetMembers: Array.Empty<MemberId>());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*target at least one member*");
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
        var routine = BuildRoutine(
            name: "Morning Stretch",
            scope: RoutineScope.Members,
            kind: RoutineKind.Scheduled,
            color: "#22C55E",
            schedule: RoutineSchedule.Weekly(new[] { DayOfWeek.Friday }, new TimeOnly(7, 0)),
            targetMembers: new[] { MemberId.New() });

        var evt = (RoutineCreated)routine.DomainEvents.Single();
        evt.RoutineId.Should().Be(routine.Id.Value);
        evt.FamilyId.Should().Be(routine.FamilyId.Value);
        evt.Name.Should().Be("Morning Stretch");
        evt.Scope.Should().Be(nameof(RoutineScope.Members));
        evt.Kind.Should().Be(nameof(RoutineKind.Scheduled));
        evt.Color.Should().Be("#22C55E");
    }

    [Fact]
    public void Create_SetsStructuredProperties()
    {
        var memberId = MemberId.New();
        var schedule = RoutineSchedule.Monthly(new[] { 1, 15 }, new TimeOnly(20, 30));

        var routine = BuildRoutine(
            name: "Review expenses",
            scope: RoutineScope.Members,
            kind: RoutineKind.Scheduled,
            color: "#EF4444",
            schedule: schedule,
            targetMembers: new[] { memberId });

        routine.Name.Value.Should().Be("Review expenses");
        routine.Scope.Should().Be(RoutineScope.Members);
        routine.Kind.Should().Be(RoutineKind.Scheduled);
        routine.Color.Value.Should().Be("#EF4444");
        routine.Schedule.Should().Be(schedule);
        routine.TargetMemberIds.Should().ContainSingle().Which.Should().Be(memberId);
    }

    // --- Update ---

    [Fact]
    public void Update_ChangesStructuredFields()
    {
        var routine = BuildRoutine();
        var memberId = MemberId.New();
        routine.ClearDomainEvents();

        var newSchedule = RoutineSchedule.Yearly(9, new[] { 1 });

        routine.Update(
            RoutineName.Create("Prepare ski trip"),
            RoutineScope.Members,
            RoutineKind.Cue,
            HexColor.From("#0EA5E9"),
            newSchedule,
            new[] { memberId });

        routine.Name.Value.Should().Be("Prepare ski trip");
        routine.Scope.Should().Be(RoutineScope.Members);
        routine.Kind.Should().Be(RoutineKind.Cue);
        routine.Color.Value.Should().Be("#0EA5E9");
        routine.Schedule.Should().Be(newSchedule);
        routine.TargetMemberIds.Should().ContainSingle().Which.Should().Be(memberId);
    }

    [Fact]
    public void Update_EmitsRoutineUpdatedEvent()
    {
        var routine = BuildRoutine();
        var memberId = MemberId.New();
        routine.ClearDomainEvents();

        routine.Update(
            RoutineName.Create("New Name"),
            RoutineScope.Members,
            RoutineKind.Scheduled,
            HexColor.From("#F59E0B"),
            RoutineSchedule.Weekly(new[] { DayOfWeek.Friday }, new TimeOnly(20, 30)),
            new[] { memberId });

        routine.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RoutineUpdated>();
    }

    [Fact]
    public void Update_RoutineUpdatedEvent_ContainsCorrectData()
    {
        var routine = BuildRoutine();
        var memberId = MemberId.New();
        routine.ClearDomainEvents();

        routine.Update(
            RoutineName.Create("Combo"),
            RoutineScope.Members,
            RoutineKind.Scheduled,
            HexColor.From("#F59E0B"),
            RoutineSchedule.Weekly(new[] { DayOfWeek.Friday }, new TimeOnly(20, 30)),
            new[] { memberId });

        var evt = (RoutineUpdated)routine.DomainEvents.Single();
        evt.RoutineId.Should().Be(routine.Id.Value);
        evt.Name.Should().Be("Combo");
        evt.Scope.Should().Be(nameof(RoutineScope.Members));
        evt.Kind.Should().Be(nameof(RoutineKind.Scheduled));
        evt.Color.Should().Be("#F59E0B");
    }

    [Fact]
    public void Update_WithMemberScopeAndNoTargetMembers_Throws()
    {
        var routine = BuildRoutine();

        var act = () => routine.Update(
            RoutineName.Create("Updated"),
            RoutineScope.Members,
            RoutineKind.Cue,
            HexColor.From("#A855F7"),
            RoutineSchedule.Weekly(new[] { DayOfWeek.Monday }),
            Array.Empty<MemberId>());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*target at least one member*");
    }

    // --- AppliesTo ---

    [Fact]
    public void AppliesTo_HouseholdRoutine_ReturnsTrueForAnyMember()
    {
        var routine = BuildRoutine(scope: RoutineScope.Household);

        routine.AppliesTo(MemberId.New()).Should().BeTrue();
        routine.AppliesTo(MemberId.New()).Should().BeTrue();
    }

    [Fact]
    public void AppliesTo_MemberScopedRoutine_ReturnsTrueOnlyForTargetMembers()
    {
        var memberA = MemberId.New();
        var memberB = MemberId.New();

        var routine = BuildRoutine(
            scope: RoutineScope.Members,
            targetMembers: new[] { memberA });

        routine.AppliesTo(memberA).Should().BeTrue();
        routine.AppliesTo(memberB).Should().BeFalse();
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

    // --- RoutineSchedule.Daily ---

    [Fact]
    public void RoutineSchedule_Daily_HasCorrectFrequency()
    {
        var schedule = RoutineSchedule.Daily();

        schedule.Frequency.Should().Be(RoutineFrequency.Daily);
        schedule.DaysOfWeek.Should().BeEmpty();
        schedule.DaysOfMonth.Should().BeEmpty();
        schedule.MonthOfYear.Should().BeNull();
        schedule.Time.Should().BeNull();
    }

    [Fact]
    public void RoutineSchedule_Daily_WithTime_PreservesTime()
    {
        var time = new TimeOnly(8, 0);

        var schedule = RoutineSchedule.Daily(time);

        schedule.Frequency.Should().Be(RoutineFrequency.Daily);
        schedule.Time.Should().Be(time);
    }

    [Fact]
    public void RoutineSchedule_Daily_OccursOn_ReturnsTrueForAnyDate()
    {
        var schedule = RoutineSchedule.Daily();

        schedule.OccursOn(DateOnly.FromDateTime(DateTime.UtcNow)).Should().BeTrue();
        schedule.OccursOn(new DateOnly(2000, 1, 1)).Should().BeTrue();
        schedule.OccursOn(new DateOnly(2099, 12, 31)).Should().BeTrue();
    }
}