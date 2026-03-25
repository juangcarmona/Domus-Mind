namespace DomusMind.Application.Features.SharedLists;

public enum SharedListErrorCode
{
    InvalidInput,
    AccessDenied,
    ListNotFound,
    ItemNotFound,
}

public sealed class SharedListException : Exception
{
    public SharedListErrorCode Code { get; }

    public SharedListException(SharedListErrorCode code, string message) : base(message)
    {
        Code = code;
    }
}
