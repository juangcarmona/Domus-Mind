namespace DomusMind.Contracts.Responsibilities;

public sealed record MemberResponsibilityLoad(
    Guid MemberId,
    string MemberName,
    int PrimaryOwnerships,
    int SecondaryOwnerships,
    int TotalLoad);

public sealed record ResponsibilityBalanceResponse(
    IReadOnlyCollection<MemberResponsibilityLoad> Loads);
