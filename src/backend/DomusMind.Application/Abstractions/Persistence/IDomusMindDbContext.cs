namespace DomusMind.Application.Abstractions.Persistence;

using Microsoft.EntityFrameworkCore;

public interface IDomusMindDbContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    /// <summary>Sets an EF shadow property on a tracked entity. Used for infrastructure-only columns not modelled on the entity.</summary>
    void SetProperty<TEntity>(TEntity entity, string propertyName, object? value) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}