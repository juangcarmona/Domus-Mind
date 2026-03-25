using DomusMind.Domain.Family;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.SharedLists;
using DomusMind.Domain.SharedLists.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.SharedLists;

public sealed class SharedListConfiguration : IEntityTypeConfiguration<SharedList>
{
    public void Configure(EntityTypeBuilder<SharedList> builder)
    {
        builder.ToTable("shared_lists");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasConversion(id => id.Value, value => SharedListId.From(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(l => l.FamilyId)
            .HasConversion(id => id.Value, value => FamilyId.From(value))
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(l => l.Name)
            .HasConversion(name => name.Value, value => SharedListName.Create(value))
            .HasColumnName("name")
            .HasMaxLength(SharedListName.MaxLength)
            .IsRequired();

        builder.Property(l => l.Kind)
            .HasConversion(k => k.Value, value => SharedListKind.Create(value))
            .HasColumnName("kind")
            .HasMaxLength(SharedListKind.MaxLength)
            .IsRequired();

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

        builder.HasMany(l => l.Items)
            .WithOne()
            .HasForeignKey("SharedListId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(l => l.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(l => l.DomainEvents);
    }
}
