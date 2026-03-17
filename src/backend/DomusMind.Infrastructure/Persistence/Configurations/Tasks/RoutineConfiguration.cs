using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.Tasks;

public sealed class RoutineConfiguration : IEntityTypeConfiguration<Routine>
{
    public void Configure(EntityTypeBuilder<Routine> builder)
    {
        builder.ToTable("routines");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => RoutineId.From(value))
            .HasColumnName("id")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(r => r.FamilyId)
            .HasConversion(id => id.Value, value => FamilyId.From(value))
            .HasColumnName("family_id")
            .IsRequired();

        builder.OwnsOne(r => r.Name, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.Property(r => r.Scope)
            .HasConversion<string>()
            .HasColumnName("scope")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.Kind)
            .HasConversion<string>()
            .HasColumnName("kind")
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsOne(r => r.Color, color =>
        {
            color.Property(c => c.Value)
                .HasColumnName("color")
                .HasMaxLength(7)
                .IsRequired();
        });

        builder.OwnsOne(r => r.Schedule, schedule =>
        {
            schedule.Property(s => s.Frequency)
                .HasConversion<string>()
                .HasColumnName("schedule_frequency")
                .HasMaxLength(20)
                .IsRequired();

            schedule.Property(s => s.MonthOfYear)
                .HasColumnName("schedule_month_of_year");

            schedule.Property(s => s.Time)
                .HasColumnName("schedule_time");

            schedule.Property(s => s.DaysOfWeek)
                .HasConversion(
                    v => string.Join(",", v.Select(x => (int)x)),
                    v => string.IsNullOrWhiteSpace(v)
                        ? Array.Empty<DayOfWeek>()
                        : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => (DayOfWeek)int.Parse(x))
                            .ToArray())
                .HasColumnName("schedule_days_of_week")
                .HasColumnType("text")
                .IsRequired();

            schedule.Property(s => s.DaysOfMonth)
                .HasConversion(
                    v => string.Join(",", v),
                    v => string.IsNullOrWhiteSpace(v)
                        ? Array.Empty<int>()
                        : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(int.Parse)
                            .ToArray())
                .HasColumnName("schedule_days_of_month")
                .HasColumnType("text")
                .IsRequired();
        });

        builder.OwnsMany<RoutineTargetMember>("_targetMembers", members =>
        {
            members.ToTable("routine_target_members");

            members.WithOwner()
                .HasForeignKey("routine_id");

            members.Property<RoutineId>("routine_id")
                .HasConversion(id => id.Value, value => RoutineId.From(value))
                .HasColumnName("routine_id")
                .IsRequired();

            members.Property(x => x.Id)
                .HasConversion(id => id.Value, value => MemberId.From(value))
                .HasColumnName("member_id")
                .IsRequired();

            members.Ignore(x => x.MemberId);

            members.HasKey("routine_id", "Id");

            members.HasIndex("Id");
            members.HasIndex("routine_id", "Id").IsUnique();
        });

        builder.Navigation("_targetMembers")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(r => r.TargetMemberIds);
        builder.Ignore(r => r.DomainEvents);

        builder.HasIndex(r => r.FamilyId);
        builder.HasIndex(r => new { r.FamilyId, r.Status });
    }
}