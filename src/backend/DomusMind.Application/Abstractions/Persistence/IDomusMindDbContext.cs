namespace DomusMind.Application.Abstractions.Persistence;

using DomusMind.Domain.Calendar.ExternalConnections;
using Microsoft.EntityFrameworkCore;

public interface IDomusMindDbContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the encrypted MSAL token cache and granted scopes into the shadow properties
    /// on a tracked ExternalCalendarConnection before SaveChanges is called.
    /// This is an infrastructure concern exposed here because the Application layer must
    /// persist auth material alongside the connection row in a single SaveChanges call,
    /// but shadow property access requires EF Change Tracker access.
    /// </summary>
    void SetExternalCalendarConnectionAuthMaterial(
        ExternalCalendarConnection connection,
        string encryptedTokenCache,
        string grantedScopes);
}