using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.EventLog;

public sealed class EventLogEntryConfiguration : IEntityTypeConfiguration<EventLogEntry>
{
    public void Configure(EntityTypeBuilder<EventLogEntry> builder)
    {
        builder.ToTable("event_log");

        builder.HasKey(x => x.EventId);

        builder.Property(x => x.EventType)
            .IsRequired();

        builder.Property(x => x.AggregateType)
            .IsRequired();

        builder.Property(x => x.AggregateId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.Module)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.OccurredAtUtc)
            .IsRequired();

        builder.Property(x => x.Version)
            .IsRequired();

        builder.Property(x => x.PayloadJson)
            .IsRequired();

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(128);

        builder.Property(x => x.CausationId)
            .HasMaxLength(128);

        builder.HasIndex(x => x.OccurredAtUtc);
        builder.HasIndex(x => new { x.Module, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.AggregateType, x.AggregateId, x.Version });
    }
}