namespace DomusMind.Application.Abstractions.Security;

/// <summary>Grants a user access to a family. Infrastructure-only concern; does not touch the domain.</summary>
public interface IFamilyAccessGranter
{
    /// <summary>
    /// Records that <paramref name="userId"/> may access <paramref name="familyId"/>.
    /// The caller is responsible for persisting via DbContext.SaveChangesAsync.
    /// </summary>
    Task GrantAccessAsync(Guid userId, Guid familyId, CancellationToken cancellationToken = default);
}
