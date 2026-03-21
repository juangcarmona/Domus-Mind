using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Calendar.Events;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Domain.Shared;

namespace DomusMind.Domain.Calendar;

public sealed class CalendarEvent : AggregateRoot<CalendarEventId>
{
    private readonly List<MemberId> _participantIds = [];
    private readonly List<int> _reminderOffsets = [];

    public FamilyId FamilyId { get; private set; }
    public EventTitle Title { get; private set; }
    public string? Description { get; private set; }
    public EventTime Time { get; private set; }
    public HexColor Color { get; private set; }
    public EventStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<MemberId> ParticipantIds => _participantIds.AsReadOnly();
    public IReadOnlyCollection<int> ReminderOffsets => _reminderOffsets.AsReadOnly();

    private CalendarEvent(
        CalendarEventId id,
        FamilyId familyId,
        EventTitle title,
        string? description,
        EventTime time,
        HexColor color,
        DateTime createdAtUtc)
        : base(id)
    {
        FamilyId = familyId;
        Title = title;
        Description = description;
        Time = time;
        Color = color;
        Status = EventStatus.Scheduled;
        CreatedAtUtc = createdAtUtc;
    }

    public static CalendarEvent Create(
        CalendarEventId id,
        FamilyId familyId,
        EventTitle title,
        string? description,
        EventTime time,
        HexColor color,
        DateTime createdAtUtc)
    {
        var calendarEvent = new CalendarEvent(id, familyId, title, description, time, color, createdAtUtc);
        calendarEvent.RaiseDomainEvent(new EventScheduled(
            Guid.NewGuid(), id.Value, familyId.Value, title.Value, time, createdAtUtc));
        return calendarEvent;
    }

    public void Reschedule(EventTime newTime)
    {
        if (Status == EventStatus.Cancelled)
            throw new InvalidOperationException("Cannot reschedule a cancelled event.");

        Time = newTime;

        RaiseDomainEvent(new EventRescheduled(
            Guid.NewGuid(), Id.Value, newTime, DateTime.UtcNow));
    }

    public void Edit(EventTitle newTitle, string? newDescription)
    {
        if (Status == EventStatus.Cancelled)
            throw new InvalidOperationException("Cannot edit a cancelled event.");

        Title = newTitle;
        Description = string.IsNullOrWhiteSpace(newDescription) ? null : newDescription.Trim();
    }

    public void Repaint(HexColor newColor)
    {
        if (Status == EventStatus.Cancelled)
            throw new InvalidOperationException("Cannot repaint a cancelled event.");

        Color = newColor;
    }

    public void Cancel()
    {
        if (Status == EventStatus.Cancelled)
            throw new InvalidOperationException("Event is already cancelled.");

        Status = EventStatus.Cancelled;

        RaiseDomainEvent(new EventCancelled(
            Guid.NewGuid(), Id.Value, DateTime.UtcNow));
    }

    public void AddParticipant(MemberId memberId)
    {
        if (Status == EventStatus.Cancelled)
            throw new InvalidOperationException("Cannot add participants to a cancelled event.");

        if (_participantIds.Contains(memberId))
            throw new InvalidOperationException(
                $"Member '{memberId.Value}' is already a participant of this event.");

        _participantIds.Add(memberId);

        RaiseDomainEvent(new EventParticipantAdded(
            Guid.NewGuid(), Id.Value, memberId.Value, DateTime.UtcNow));
    }

    public void RemoveParticipant(MemberId memberId)
    {
        if (!_participantIds.Contains(memberId))
            throw new InvalidOperationException(
                $"Member '{memberId.Value}' is not a participant of this event.");

        _participantIds.Remove(memberId);

        RaiseDomainEvent(new EventParticipantRemoved(
            Guid.NewGuid(), Id.Value, memberId.Value, DateTime.UtcNow));
    }

    public void AddReminder(int minutesBefore)
    {
        if (minutesBefore <= 0)
            throw new InvalidOperationException("Reminder offset must be greater than zero.");

        if (_reminderOffsets.Contains(minutesBefore))
            throw new InvalidOperationException(
                $"A reminder with offset {minutesBefore} minutes already exists for this event.");

        _reminderOffsets.Add(minutesBefore);

        RaiseDomainEvent(new ReminderAdded(
            Guid.NewGuid(), Id.Value, minutesBefore, DateTime.UtcNow));
    }

    public void RemoveReminder(int minutesBefore)
    {
        if (!_reminderOffsets.Contains(minutesBefore))
            throw new InvalidOperationException(
                $"No reminder with offset {minutesBefore} minutes exists for this event.");

        _reminderOffsets.Remove(minutesBefore);

        RaiseDomainEvent(new ReminderRemoved(
            Guid.NewGuid(), Id.Value, minutesBefore, DateTime.UtcNow));
    }

#pragma warning disable CS8618
    // EF Core parameterless constructor
    private CalendarEvent() : base(default) { }
#pragma warning restore CS8618
}
