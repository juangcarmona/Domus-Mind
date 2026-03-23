namespace DomusMind.Application.Features.Setup;

public enum SetupErrorCode
{
    AlreadyInitialized,
    WeakPassword,
    EmailAlreadyTaken,
}

public sealed class SetupException : Exception
{
    public SetupErrorCode Code { get; }

    public SetupException(SetupErrorCode code, string message)
        : base(message)
    {
        Code = code;
    }
}
