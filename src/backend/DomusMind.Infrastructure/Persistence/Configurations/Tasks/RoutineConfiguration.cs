using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.ValueObjects;
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
            .IsRequired();

        builder.Property(r => r.FamilyId)
            .HasConversion(id => id.Value, value => FamilyId.From(value))
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(r => r.Name)
            .HasConversion(name => name.Value, value => RoutineName.Create(value))
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Cadence)
            .HasColumnName("cadence")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Ignore(r => r.DomainEvents);
    }
}
