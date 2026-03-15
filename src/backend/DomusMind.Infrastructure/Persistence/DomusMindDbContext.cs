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

    public DbSet<RefreshTokenRecord> RefreshTokens => Set<RefreshTokenRecord>();

    public DbSet<Domain.Family.Family> Families => Set<Domain.Family.Family>();

    public DbSet<UserFamilyAccess> UserFamilyAccesses => Set<UserFamilyAccess>();

    public DbSet<Domain.Responsibilities.ResponsibilityDomain> ResponsibilityDomains
        => Set<Domain.Responsibilities.ResponsibilityDomain>();

    public DbSet<Domain.Calendar.CalendarEvent> CalendarEvents
        => Set<Domain.Calendar.CalendarEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DomusMindDbContext).Assembly);
    }
}