namespace DomusMind.Contracts.Responsibilities;

public sealed record OverloadedMember(
    Guid MemberId,
    string MemberName,
    int TotalLoad,
    IReadOnlyCollection<string> DomainNames);

public sealed record ResponsibilityOverloadResponse(
    int Threshold,
    IReadOnlyCollection<OverloadedMember> Overloaded);
