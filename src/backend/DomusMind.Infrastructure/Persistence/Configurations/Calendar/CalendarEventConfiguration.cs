using System.Text.Json;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Domain.Responsibilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DomusMind.Infrastructure.Persistence.Configurations.Calendar;

public sealed class CalendarEventConfiguration : IEntityTypeConfiguration<Domain.Calendar.CalendarEvent>
{
    public void Configure(EntityTypeBuilder<Domain.Calendar.CalendarEvent> builder)
    {
        builder.ToTable("calendar_events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => CalendarEventId.From(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.FamilyId)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value))
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(e => e.Title)
            .HasConversion(
                title => title.Value,
                value => EventTitle.Create(value))
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        // EventTime stored as five columns: kind + date + time + end_date + end_time
        builder.OwnsOne(e => e.Time, time =>
        {
            time.Property(t => t.Kind)
                .HasConversion<string>()
                .HasColumnName("event_time_kind")
                .HasMaxLength(20)
                .IsRequired();

            time.Property(t => t.Date)
                .HasColumnName("event_date")
                .IsRequired();

            time.Property(t => t.Time)
                .HasColumnName("event_time");

            time.Property(t => t.EndDate)
                .HasColumnName("event_end_date");

            time.Property(t => t.EndTime)
                .HasColumnName("event_end_time");
        });

        builder.OwnsOne(e => e.Color, color =>
        {
            color.Property(c => c.Value)
                .HasColumnName("color")
                .HasMaxLength(7)
                .IsRequired();
        });

        // Participant IDs stored as JSON text - list of member GUIDs.
        var participantConverter = new ValueConverter<List<MemberId>, string>(
            ids => JsonSerializer.Serialize(ids.Select(id => id.Value).ToList(), (JsonSerializerOptions?)null),
            json => JsonSerializer.Deserialize<List<Guid>>(json, (JsonSerializerOptions?)null)!
                .Select(MemberId.From).ToList());

        var participantComparer = new ValueComparer<List<MemberId>>(
            (x, y) => x != null && y != null && x.SequenceEqual(y),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property<List<MemberId>>("_participantIds")
            .HasField("_participantIds")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasConversion(participantConverter, participantComparer)
            .HasColumnName("participant_ids")
            .HasColumnType("text")
            .IsRequired();

        builder.Ignore(e => e.ParticipantIds);

        // Reminder offsets stored as JSON text - list of int (minutes before).
        var reminderConverter = new ValueConverter<List<int>, string>(
            offsets => JsonSerializer.Serialize(offsets, (JsonSerializerOptions?)null),
            json => JsonSerializer.Deserialize<List<int>>(json, (JsonSerializerOptions?)null)!);

        var reminderComparer = new ValueComparer<List<int>>(
            (x, y) => x != null && y != null && x.SequenceEqual(y),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property<List<int>>("_reminderOffsets")
            .HasField("_reminderOffsets")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasConversion(reminderConverter, reminderComparer)
            .HasColumnName("reminder_offsets")
            .HasColumnType("text")
            .IsRequired();

        builder.Ignore(e => e.ReminderOffsets);

        builder.Property(e => e.AreaId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? ResponsibilityDomainId.From(value.Value) : (ResponsibilityDomainId?)null)
            .HasColumnName("area_id");

        builder.Ignore(e => e.DomainEvents);
    }
}
