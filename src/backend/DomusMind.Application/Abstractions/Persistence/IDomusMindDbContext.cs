namespace DomusMind.Application.Abstractions.Persistence;

using Microsoft.EntityFrameworkCore;

public interface IDomusMindDbContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}