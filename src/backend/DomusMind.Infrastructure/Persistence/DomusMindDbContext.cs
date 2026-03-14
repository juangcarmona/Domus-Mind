using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Infrastructure.Persistence;

public sealed class DomusMindDbContext : DbContext, IDomusMindDbContext
{
    public DomusMindDbContext(DbContextOptions<DomusMindDbContext> options)
        : base(options)
    {
    }

    public DbSet<EventLogEntry> EventLog => Set<EventLogEntry>();

    public DbSet<AuthUser> AuthUsers => Set<AuthUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DomusMindDbContext).Assembly);
    }
}