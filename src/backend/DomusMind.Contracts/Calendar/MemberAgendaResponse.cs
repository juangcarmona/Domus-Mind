namespace DomusMind.Contracts.Calendar;

public sealed record MemberAgendaResponse(
    Guid MemberId,
    string Mode,
    DateTime WindowStartUtc,
    DateTime WindowEndUtc,
    IReadOnlyCollection<MemberAgendaItem> Items);
