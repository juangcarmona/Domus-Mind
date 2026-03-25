using DomusMind.Domain.Family;
using DomusMind.Domain.SharedLists;
using DomusMind.Domain.SharedLists.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.SharedLists;

public sealed class SharedListItemConfiguration : IEntityTypeConfiguration<SharedListItem>
{
    public void Configure(EntityTypeBuilder<SharedListItem> builder)
    {
        builder.ToTable("shared_list_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasConversion(id => id.Value, value => SharedListItemId.From(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(i => i.Name)
            .HasConversion(name => name.Value, value => SharedListItemName.Create(value))
            .HasColumnName("name")
            .HasMaxLength(SharedListItemName.MaxLength)
            .IsRequired();

        builder.Property(i => i.Checked)
            .HasColumnName("checked")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .HasColumnName("quantity")
            .HasMaxLength(100);

        builder.Property(i => i.Note)
            .HasColumnName("note")
            .HasMaxLength(500);

        builder.Property(i => i.Order)
            .HasColumnName("order")
            .IsRequired();

        builder.Property(i => i.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.Property(i => i.UpdatedByMemberId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? MemberId.From(value.Value) : null)
            .HasColumnName("updated_by_member_id");

        // Shadow FK to parent list
        builder.Property<SharedListId>("SharedListId")
            .HasConversion(id => id.Value, value => SharedListId.From(value))
            .HasColumnName("shared_list_id")
            .IsRequired();
    }
}
