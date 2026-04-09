using DomusMind.Domain.Calendar.ExternalConnections;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.Calendar;

public sealed class ExternalCalendarConnectionConfiguration
    : IEntityTypeConfiguration<ExternalCalendarConnection>
{
    public void Configure(EntityTypeBuilder<ExternalCalendarConnection> builder)
    {
        builder.ToTable("external_calendar_connections");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => ExternalCalendarConnectionId.From(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(c => c.FamilyId)
            .HasConversion(id => id.Value, value => FamilyId.From(value))
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(c => c.MemberId)
            .HasConversion(id => id.Value, value => MemberId.From(value))
            .HasColumnName("member_id")
            .IsRequired();

        builder.Property(c => c.Provider)
            .HasConversion<string>()
            .HasColumnName("provider")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(c => c.ProviderAccountId)
            .HasColumnName("provider_account_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.AccountEmail)
            .HasColumnName("account_email")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(c => c.AccountDisplayLabel)
            .HasColumnName("account_display_label")
            .HasMaxLength(100);

        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100);

        builder.OwnsOne(c => c.Horizon, h =>
        {
            h.Property(s => s.ForwardHorizonDays)
                .HasColumnName("forward_horizon_days")
                .IsRequired();
        });

        builder.Property(c => c.ScheduledRefreshEnabled)
            .HasColumnName("scheduled_refresh_enabled")
            .IsRequired();

        builder.Property(c => c.ScheduledRefreshIntervalMinutes)
            .HasColumnName("scheduled_refresh_interval_minutes")
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(c => c.LastSuccessfulSyncUtc)
            .HasColumnName("last_successful_sync_utc");

        builder.Property(c => c.LastSyncAttemptUtc)
            .HasColumnName("last_sync_attempt_utc");

        builder.Property(c => c.LastSyncFailureUtc)
            .HasColumnName("last_sync_failure_utc");

        builder.Property(c => c.LastErrorCode)
            .HasColumnName("last_error_code")
            .HasMaxLength(50);

        builder.Property(c => c.LastErrorMessage)
            .HasColumnName("last_error_message")
            .HasMaxLength(500);

        builder.Property(c => c.NextScheduledSyncUtc)
            .HasColumnName("next_scheduled_sync_utc");

        builder.Property(c => c.SyncLeaseId)
            .HasColumnName("sync_lease_id");

        builder.Property(c => c.SyncLeaseExpiresAtUtc)
            .HasColumnName("sync_lease_expires_at_utc");

        builder.Property(c => c.Version)
            .HasColumnName("version")
            .IsRequired();

        builder.Property(c => c.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(c => c.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.HasMany(c => c.Feeds)
            .WithOne()
            .HasPrincipalKey(c => c.Id)
            .HasForeignKey(f => f.ConnectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Feeds)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(c => c.DomainEvents);

        builder.HasIndex(c => new { c.MemberId, c.Provider })
            .HasDatabaseName("ix_external_calendar_connections_member_provider");

        builder.HasIndex(c => c.NextScheduledSyncUtc)
            .HasDatabaseName("ix_external_calendar_connections_next_sync");

        builder.HasIndex(c => c.SyncLeaseExpiresAtUtc)
            .HasDatabaseName("ix_external_calendar_connections_lease_expires");

        // Auth metadata — stored as shadow properties (infrastructure concern, not domain).
        builder.Property<string?>("EncryptedRefreshToken")
            .HasColumnName("encrypted_refresh_token")
            .HasColumnType("text");

        builder.Property<string?>("CachedAccessToken")
            .HasColumnName("cached_access_token")
            .HasColumnType("text");

        builder.Property<DateTime?>("AccessTokenExpiresAtUtc")
            .HasColumnName("access_token_expires_at_utc");

        builder.Property<string?>("GrantedScopes")
            .HasColumnName("granted_scopes")
            .HasMaxLength(500);
    }
}
