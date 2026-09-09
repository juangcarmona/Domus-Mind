namespace DomusMind.Domain.Calendar.ExternalConnections;

public enum ExternalCalendarProvider
{
    Microsoft
}

public static class ExternalCalendarProviderNames
{
    public const string Microsoft = "microsoft";

    public static string ToProviderString(ExternalCalendarProvider provider) => provider switch
    {
        ExternalCalendarProvider.Microsoft => Microsoft,
        _ => throw new ArgumentOutOfRangeException(nameof(provider))
    };

    public static string ToProviderLabel(ExternalCalendarProvider provider) => provider switch
    {
        ExternalCalendarProvider.Microsoft => "Outlook",
        _ => throw new ArgumentOutOfRangeException(nameof(provider))
    };
}
