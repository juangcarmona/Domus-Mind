namespace DomusMind.Application.Features.Family;

public enum FamilyErrorCode
{
    FamilyNotFound,
    AccessDenied,
    MemberAlreadyExists,
    MemberNotFound,
    InvalidInput,
    FamilyAlreadyExists,
    HouseholdCreationNotAllowed,
}

public sealed class FamilyException : Exception
{
    public FamilyErrorCode Code { get; }
    public string? PolicyReasonCode { get; }

    public FamilyException(FamilyErrorCode code, string message) : base(message)
    {
        Code = code;
    }

    public FamilyException(FamilyErrorCode code, string message, string? policyReasonCode) : base(message)
    {
        Code = code;
        PolicyReasonCode = policyReasonCode;
    }
}
