using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.Events;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using FluentAssertions;

namespace DomusMind.Domain.Tests.Calendar;

public sealed class CalendarEventTests
{
    private static Domain.Calendar.CalendarEvent BuildEvent(
        string title = "School Excursion",
        DateTime? startTime = null,
        DateTime? endTime = null)
    {
        var id = CalendarEventId.New();
        var familyId = FamilyId.New();
        var eventTitle = EventTitle.Create(title);
        var start = startTime ?? DateTime.UtcNow.AddDays(1);
        return Domain.Calendar.CalendarEvent.Create(id, familyId, eventTitle, null, start, endTime, DateTime.UtcNow);
    }

    // ── Create ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_GivenValidInputs_SetsTitle()
    {
        var evt = BuildEvent("Doctor Appointment");

        evt.Title.Value.Should().Be("Doctor Appointment");
    }

    [Fact]
    public void Create_SetsStatusToScheduled()
    {
        var evt = BuildEvent();

        evt.Status.Should().Be(EventStatus.Scheduled);
    }

    [Fact]
    public void Create_SetsCreatedAtUtc()
    {
        var before = DateTime.UtcNow;
        var evt = BuildEvent();
        var after = DateTime.UtcNow;

        evt.CreatedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Create_StartsWithNoParticipants()
    {
        var evt = BuildEvent();

        evt.ParticipantIds.Should().BeEmpty();
    }

    [Fact]
    public void Create_StartsWithNoReminders()
    {
        var evt = BuildEvent();

        evt.ReminderOffsets.Should().BeEmpty();
    }

    [Fact]
    public void Create_EmitsOneEventScheduledEvent()
    {
        var evt = BuildEvent();

        evt.DomainEvents.Should().HaveCount(1);
        evt.DomainEvents.Single().Should().BeOfType<EventScheduled>();
    }

    [Fact]
    public void Create_EventScheduled_ContainsCorrectIds()
    {
        var id = CalendarEventId.New();
        var familyId = FamilyId.New();
        var start = DateTime.UtcNow.AddHours(2);
        var evt = Domain.Calendar.CalendarEvent.Create(
            id, familyId, EventTitle.Create("Trip"), null, start, null, DateTime.UtcNow);

        var domainEvt = evt.DomainEvents.OfType<EventScheduled>().Single();
        domainEvt.CalendarEventId.Should().Be(id.Value);
        domainEvt.FamilyId.Should().Be(familyId.Value);
        domainEvt.Title.Should().Be("Trip");
        domainEvt.StartTime.Should().Be(start);
    }

    [Fact]
    public void Create_EndTimeBeforeStartTime_ThrowsInvalidOperationException()
    {
        var start = DateTime.UtcNow.AddHours(2);
        var end = DateTime.UtcNow.AddHours(1);

        var act = () => Domain.Calendar.CalendarEvent.Create(
            CalendarEventId.New(), FamilyId.New(), EventTitle.Create("Bad"), null, start, end, DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    // ── EventTitle value object ────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EventTitle_EmptyValue_ThrowsArgumentException(string value)
    {
        var act = () => EventTitle.Create(value);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EventTitle_ValueTooLong_ThrowsArgumentException()
    {
        var act = () => EventTitle.Create(new string('X', 201));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EventTitle_TrimsWhitespace()
    {
        var title = EventTitle.Create("  Trip  ");

        title.Value.Should().Be("Trip");
    }

    // ── Reschedule ─────────────────────────────────────────────────────────────

    [Fact]
    public void Reschedule_UpdatesStartTime()
    {
        var evt = BuildEvent();
        var newStart = DateTime.UtcNow.AddDays(3);
        evt.ClearDomainEvents();

        evt.Reschedule(newStart, null);

        evt.StartTime.Should().Be(newStart);
    }

    [Fact]
    public void Reschedule_EmitsEventRescheduledEvent()
    {
        var evt = BuildEvent();
        evt.ClearDomainEvents();
        var newStart = DateTime.UtcNow.AddDays(3);

        evt.Reschedule(newStart, null);

        evt.DomainEvents.Should().HaveCount(1);
        evt.DomainEvents.Single().Should().BeOfType<EventRescheduled>();
    }

    [Fact]
    public void Reschedule_EventRescheduled_ContainsCorrectTimes()
    {
        var evt = BuildEvent();
        evt.ClearDomainEvents();
        var newStart = DateTime.UtcNow.AddDays(3);
        var newEnd = newStart.AddHours(2);

        evt.Reschedule(newStart, newEnd);

        var domainEvt = evt.DomainEvents.OfType<EventRescheduled>().Single();
        domainEvt.NewStartTime.Should().Be(newStart);
        domainEvt.NewEndTime.Should().Be(newEnd);
        domainEvt.CalendarEventId.Should().Be(evt.Id.Value);
    }

    [Fact]
    public void Reschedule_CancelledEvent_ThrowsInvalidOperationException()
    {
        var evt = BuildEvent();
        evt.Cancel();

        var act = () => evt.Reschedule(DateTime.UtcNow.AddDays(1), null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reschedule_NewEndBeforeNewStart_ThrowsInvalidOperationException()
    {
        var evt = BuildEvent();
        var newStart = DateTime.UtcNow.AddHours(5);
        var newEnd = DateTime.UtcNow.AddHours(3);

        var act = () => evt.Reschedule(newStart, newEnd);

        act.Should().Throw<InvalidOperationException>();
    }

    // ── Cancel ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_SetsStatusToCancelled()
    {
        var evt = BuildEvent();

        evt.Cancel();

        evt.Status.Should().Be(EventStatus.Cancelled);
    }

    [Fact]
    public void Cancel_EmitsEventCancelledEvent()
    {
        var evt = BuildEvent();
        evt.ClearDomainEvents();

        evt.Cancel();

        evt.DomainEvents.Should().HaveCount(1);
        evt.DomainEvents.Single().Should().BeOfType<EventCancelled>();
    }

    [Fact]
    public void Cancel_AlreadyCancelled_ThrowsInvalidOperationException()
    {
        var evt = BuildEvent();
        evt.Cancel();

        var act = () => evt.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }

    // ── AddParticipant ─────────────────────────────────────────────────────────

    [Fact]
    public void AddParticipant_AddsToParticipants()
    {
        var evt = BuildEvent();
        var memberId = MemberId.New();

        evt.AddParticipant(memberId);

        evt.ParticipantIds.Should().Contain(memberId);
    }

    [Fact]
    public void AddParticipant_EmitsEventParticipantAddedEvent()
    {
        var evt = BuildEvent();
        evt.ClearDomainEvents();
        var memberId = MemberId.New();

        evt.AddParticipant(memberId);

        evt.DomainEvents.Should().HaveCount(1);
        evt.DomainEvents.Single().Should().BeOfType<EventParticipantAdded>();
    }

    [Fact]
    public void AddParticipant_Event_ContainsCorrectMemberId()
    {
        var evt = BuildEvent();
        evt.ClearDomainEvents();
        var memberId = MemberId.New();

        evt.AddParticipant(memberId);

        var domainEvt = evt.DomainEvents.OfType<EventParticipantAdded>().Single();
        domainEvt.MemberId.Should().Be(memberId.Value);
        domainEvt.CalendarEventId.Should().Be(evt.Id.Value);
    }

    [Fact]
    public void AddParticipant_DuplicateMemberId_ThrowsInvalidOperationException()
    {
        var evt = BuildEvent();
        var memberId = MemberId.New();
        evt.AddParticipant(memberId);

        var act = () => evt.AddParticipant(memberId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddParticipant_CancelledEvent_ThrowsInvalidOperationException()
    {
        var evt = BuildEvent();
        evt.Cancel();

        var act = () => evt.AddParticipant(MemberId.New());

        act.Should().Throw<InvalidOperationException>();
    }

    // ── RemoveParticipant ──────────────────────────────────────────────────────

    [Fact]
    public void RemoveParticipant_RemovesFromParticipants()
    {
        var evt = BuildEvent();
        var memberId = MemberId.New();
        evt.AddParticipant(memberId);
        evt.ClearDomainEvents();

        evt.RemoveParticipant(memberId);

        evt.ParticipantIds.Should().NotContain(memberId);
    }

    [Fact]
    public void RemoveParticipant_EmitsEventParticipantRemovedEvent()
    {
        var evt = BuildEvent();
        var memberId = MemberId.New();
        evt.AddParticipant(memberId);
        evt.ClearDomainEvents();

        evt.RemoveParticipant(memberId);

        evt.DomainEvents.Should().HaveCount(1);
        evt.DomainEvents.Single().Should().BeOfType<EventParticipantRemoved>();
    }

    [Fact]
    public void RemoveParticipant_ParticipantNotFound_ThrowsInvalidOperationException()
    {
        var evt = BuildEvent();

        var act = () => evt.RemoveParticipant(MemberId.New());

        act.Should().Throw<InvalidOperationException>();
    }

    // ── AddReminder ────────────────────────────────────────────────────────────

    [Fact]
    public void AddReminder_AddsToReminderOffsets()
    {
        var evt = BuildEvent();

        evt.AddReminder(30);

        evt.ReminderOffsets.Should().Contain(30);
    }

    [Fact]
    public void AddReminder_EmitsReminderAddedEvent()
    {
        var evt = BuildEvent();
        evt.ClearDomainEvents();

        evt.AddReminder(60);

        evt.DomainEvents.Should().HaveCount(1);
        evt.DomainEvents.Single().Should().BeOfType<ReminderAdded>();
    }

    [Fact]
    public void AddReminder_Event_ContainsCorrectOffset()
    {
        var evt = BuildEvent();
        evt.ClearDomainEvents();

        evt.AddReminder(120);

        var domainEvt = evt.DomainEvents.OfType<ReminderAdded>().Single();
        domainEvt.MinutesBefore.Should().Be(120);
        domainEvt.CalendarEventId.Should().Be(evt.Id.Value);
    }

    [Fact]
    public void AddReminder_MultipleDistinctOffsets_AddsAll()
    {
        var evt = BuildEvent();

        evt.AddReminder(30);
        evt.AddReminder(60);
        evt.AddReminder(1440);

        evt.ReminderOffsets.Should().HaveCount(3);
    }

    [Fact]
    public void AddReminder_DuplicateOffset_ThrowsInvalidOperationException()
    {
        var evt = BuildEvent();
        evt.AddReminder(30);

        var act = () => evt.AddReminder(30);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-60)]
    public void AddReminder_InvalidOffset_ThrowsInvalidOperationException(int offset)
    {
        var evt = BuildEvent();

        var act = () => evt.AddReminder(offset);

        act.Should().Throw<InvalidOperationException>();
    }

    // ── RemoveReminder ─────────────────────────────────────────────────────────

    [Fact]
    public void RemoveReminder_RemovesFromReminderOffsets()
    {
        var evt = BuildEvent();
        evt.AddReminder(30);
        evt.ClearDomainEvents();

        evt.RemoveReminder(30);

        evt.ReminderOffsets.Should().NotContain(30);
    }

    [Fact]
    public void RemoveReminder_EmitsReminderRemovedEvent()
    {
        var evt = BuildEvent();
        evt.AddReminder(60);
        evt.ClearDomainEvents();

        evt.RemoveReminder(60);

        evt.DomainEvents.Should().HaveCount(1);
        evt.DomainEvents.Single().Should().BeOfType<ReminderRemoved>();
    }

    [Fact]
    public void RemoveReminder_OffsetNotFound_ThrowsInvalidOperationException()
    {
        var evt = BuildEvent();

        var act = () => evt.RemoveReminder(30);

        act.Should().Throw<InvalidOperationException>();
    }
}
