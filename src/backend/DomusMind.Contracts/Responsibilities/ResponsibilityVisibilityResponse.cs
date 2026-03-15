namespace DomusMind.Contracts.Responsibilities;

public sealed record ResponsibilityConnection(
    Guid DomainId,
    string DomainName,
    string Role);

public sealed record MemberResponsibilityView(
    Guid MemberId,
    string MemberName,
    IReadOnlyCollection<ResponsibilityConnection> Connections);

public sealed record ResponsibilityVisibilityResponse(
    IReadOnlyCollection<MemberResponsibilityView> Views);
