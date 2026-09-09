namespace DomusMind.Application.Features.Calendar;

public enum CalendarErrorCode
{
    EventNotFound,
    AccessDenied,
    InvalidInput,
    EventAlreadyCancelled,
    DuplicateParticipant,
    ParticipantNotFound,
    DuplicateReminderOffset,
    ReminderOffsetNotFound,
    ConnectionNotFound,
    ConnectionAlreadyExists,
    ConnectionSyncInProgress,
    ConnectionAuthExpired,
    ProviderAuthFailed,
    ProviderApiError,
}

public sealed class CalendarException : Exception
{
    public CalendarErrorCode Code { get; }

    public CalendarException(CalendarErrorCode code, string message) : base(message)
    {
        Code = code;
    }
}
