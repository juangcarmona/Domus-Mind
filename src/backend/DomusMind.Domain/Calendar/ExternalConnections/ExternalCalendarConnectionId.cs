namespace DomusMind.Domain.Calendar.ExternalConnections;

public readonly record struct ExternalCalendarConnectionId(Guid Value)
{
    public static ExternalCalendarConnectionId New() => new(Guid.NewGuid());
    public static ExternalCalendarConnectionId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
