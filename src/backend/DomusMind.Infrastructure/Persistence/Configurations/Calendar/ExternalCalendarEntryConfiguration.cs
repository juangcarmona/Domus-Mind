using DomusMind.Domain.Calendar.ExternalConnections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.Calendar;

public sealed class ExternalCalendarEntryConfiguration
    : IEntityTypeConfiguration<ExternalCalendarEntry>
{
    public void Configure(EntityTypeBuilder<ExternalCalendarEntry> builder)
    {
        builder.ToTable("external_calendar_entries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.ConnectionId)
            .HasColumnName("connection_id")
            .IsRequired();

        builder.Property(e => e.FeedId)
            .HasColumnName("feed_id")
            .IsRequired();

        builder.Property(e => e.Provider)
            .HasColumnName("provider")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.ExternalEventId)
            .HasColumnName("external_event_id")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(e => e.ICalUId)
            .HasColumnName("ical_uid")
            .HasMaxLength(512);

        builder.Property(e => e.SeriesMasterId)
            .HasColumnName("series_master_id")
            .HasMaxLength(512);

        builder.Property(e => e.Title)
            .HasColumnName("title")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.StartsAtUtc)
            .HasColumnName("starts_at_utc")
            .IsRequired();

        builder.Property(e => e.EndsAtUtc)
            .HasColumnName("ends_at_utc");

        builder.Property(e => e.OriginalTimezone)
            .HasColumnName("original_timezone")
            .HasMaxLength(100);

        builder.Property(e => e.IsAllDay)
            .HasColumnName("is_all_day")
            .IsRequired();

        builder.Property(e => e.Location)
            .HasColumnName("location")
            .HasMaxLength(500);

        builder.Property(e => e.ParticipantSummaryJson)
            .HasColumnName("participant_summary_json")
            .HasColumnType("text");

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.RawPayloadHash)
            .HasColumnName("raw_payload_hash")
            .HasMaxLength(64);

        builder.Property(e => e.ProviderModifiedAtUtc)
            .HasColumnName("provider_modified_at_utc");

        builder.Property(e => e.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();

        builder.Property(e => e.OpenInProviderUrl)
            .HasColumnName("open_in_provider_url")
            .HasMaxLength(1000);

        builder.Property(e => e.LastSeenAtUtc)
            .HasColumnName("last_seen_at_utc")
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(e => e.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.HasIndex(e => new { e.ConnectionId, e.StartsAtUtc })
            .HasDatabaseName("ix_external_calendar_entries_connection_starts");

        builder.HasIndex(e => new { e.FeedId, e.StartsAtUtc, e.EndsAtUtc })
            .HasDatabaseName("ix_external_calendar_entries_feed_window");

        builder.HasIndex(e => new { e.FeedId, e.ICalUId })
            .HasDatabaseName("ix_external_calendar_entries_feed_ical_uid");

        builder.HasIndex(e => new { e.FeedId, e.ExternalEventId })
            .IsUnique()
            .HasDatabaseName("ix_external_calendar_entries_feed_event_unique");
    }
}
