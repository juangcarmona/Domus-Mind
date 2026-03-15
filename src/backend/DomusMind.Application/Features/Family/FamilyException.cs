namespace DomusMind.Application.Features.Family;

public enum FamilyErrorCode
{
    FamilyNotFound,
    AccessDenied,
    MemberAlreadyExists,
    InvalidInput,
}

public sealed class FamilyException : Exception
{
    public FamilyErrorCode Code { get; }

    public FamilyException(FamilyErrorCode code, string message) : base(message)
    {
        Code = code;
    }
}
