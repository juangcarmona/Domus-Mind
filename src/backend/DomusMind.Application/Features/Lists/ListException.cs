namespace DomusMind.Application.Features.Lists;

public enum ListErrorCode
{
    InvalidInput,
    AccessDenied,
    ListNotFound,
    ItemNotFound,
}

public sealed class ListException : Exception
{
    public ListErrorCode Code { get; }

    public ListException(ListErrorCode code, string message) : base(message)
    {
        Code = code;
    }
}
