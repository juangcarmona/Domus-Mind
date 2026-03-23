using DomusMind.Domain.Areas;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.Areas;

public sealed class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("areas");

        builder.HasKey(area => area.Id);

        builder.Property(area => area.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(area => area.FamilyId)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value))
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(area => area.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(area => area.Color)
            .HasColumnName("color")
            .HasMaxLength(16);

        builder.Property(area => area.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();
    }
}
