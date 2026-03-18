namespace DomusMind.Application.Abstractions.Security;

/// <summary>
/// Reads family access assignments for a given authenticated user.
/// </summary>
public interface IUserFamilyAccessReader
{
    /// <summary>
    /// Returns the family ID that the given user has access to, or <c>null</c> if the
    /// user has not been granted access to any family yet.
    /// </summary>
    Task<Guid?> GetFamilyIdForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
