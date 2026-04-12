using DomusMind.Domain.Family;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.Lists;

public sealed class SharedListConfiguration : IEntityTypeConfiguration<SharedList>
{
    public void Configure(EntityTypeBuilder<SharedList> builder)
    {
        builder.ToTable("shared_lists");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasConversion(id => id.Value, value => ListId.From(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(l => l.FamilyId)
            .HasConversion(id => id.Value, value => FamilyId.From(value))
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(l => l.Name)
            .HasConversion(name => name.Value, value => ListName.Create(value))
            .HasColumnName("name")
            .HasMaxLength(ListName.MaxLength)
            .IsRequired();

        builder.Property(l => l.Kind)
            .HasConversion(k => k.Value, value => ListKind.Create(value))
            .HasColumnName("kind")
            .HasMaxLength(ListKind.MaxLength)
            .IsRequired();

        builder.Property(l => l.Color)
            .HasColumnName("color")
            .HasMaxLength(50);

        builder.Property(l => l.AreaId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? ResponsibilityDomainId.From(value.Value) : null)
            .HasColumnName("area_id");

        builder.Property(l => l.LinkedEntityType)
            .HasColumnName("linked_entity_type")
            .HasMaxLength(100);

        builder.Property(l => l.LinkedEntityId)
            .HasColumnName("linked_entity_id");

        builder.Property(l => l.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(l => l.IsArchived)
            .HasColumnName("is_archived")
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasMany(l => l.Items)
            .WithOne()
            .HasForeignKey("ListId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(l => l.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(l => l.DomainEvents);
    }
}
