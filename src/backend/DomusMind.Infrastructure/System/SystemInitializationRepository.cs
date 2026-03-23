using DomusMind.Application.Abstractions.System;
using DomusMind.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Infrastructure.Initialization;

public sealed class SystemInitializationRepository : ISystemInitializationState
{
    private readonly DomusMindDbContext _db;

    public SystemInitializationRepository(DomusMindDbContext db)
    {
        _db = db;
    }

    public Task<bool> IsInitializedAsync(CancellationToken cancellationToken)
        => _db.SystemInitialization.AnyAsync(cancellationToken);

    public async Task MarkInitializedAsync(CancellationToken cancellationToken)
    {
        if (await _db.SystemInitialization.AnyAsync(cancellationToken))
            return;

        _db.SystemInitialization.Add(new SystemInitializationRecord
        {
            Id = 1,
            InitializedAtUtc = DateTimeOffset.UtcNow,
        });

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // PK violation from a concurrent first-run — already initialized, no-op.
            _db.ChangeTracker.Clear();
        }
    }
}
