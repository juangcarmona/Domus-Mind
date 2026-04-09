using DomusMind.Domain.Calendar.ExternalConnections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.Calendar;

public sealed class ExternalCalendarFeedConfiguration
    : IEntityTypeConfiguration<ExternalCalendarFeed>
{
    public void Configure(EntityTypeBuilder<ExternalCalendarFeed> builder)
    {
        builder.ToTable("external_calendar_feeds");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(f => f.ConnectionId)
            .HasConversion(id => id.Value, value => ExternalCalendarConnectionId.From(value))
            .HasColumnName("connection_id")
            .IsRequired();

        builder.Property(f => f.ProviderCalendarId)
            .HasColumnName("provider_calendar_id")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(f => f.CalendarName)
            .HasColumnName("calendar_name")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(f => f.IsDefault)
            .HasColumnName("is_default")
            .IsRequired();

        builder.Property(f => f.IsSelected)
            .HasColumnName("is_selected")
            .IsRequired();

        builder.Property(f => f.WindowStartUtc)
            .HasColumnName("window_start_utc");

        builder.Property(f => f.WindowEndUtc)
            .HasColumnName("window_end_utc");

        builder.Property(f => f.LastDeltaToken)
            .HasColumnName("last_delta_token")
            .HasMaxLength(2000);

        builder.Property(f => f.LastSuccessfulSyncUtc)
            .HasColumnName("last_successful_sync_utc");

        builder.Property(f => f.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(f => f.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.Property(f => f.ColorHex)
            .HasColumnName("color_hex")
            .HasMaxLength(7);

        builder.HasIndex(f => new { f.ConnectionId, f.IsSelected })
            .HasDatabaseName("ix_external_calendar_feeds_connection_selected");

        builder.HasIndex(f => new { f.ConnectionId, f.ProviderCalendarId })
            .IsUnique()
            .HasDatabaseName("ix_external_calendar_feeds_connection_calendar_unique");
    }
}
