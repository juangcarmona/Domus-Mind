namespace DomusMind.Application.Abstractions.Security;

public interface ICurrentUser
{
    Guid? UserId { get; }

    string? Email { get; }
}
