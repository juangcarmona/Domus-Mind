namespace DomusMind.Contracts.Calendar;

public sealed record RescheduleEventRequest(
    DateTime NewStartTime,
    DateTime? NewEndTime);
