using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.Lists;

public sealed class ListItemConfiguration : IEntityTypeConfiguration<ListItem>
{
    public void Configure(EntityTypeBuilder<ListItem> builder)
    {
        builder.ToTable("shared_list_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasConversion(id => id.Value, value => ListItemId.From(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(i => i.Name)
            .HasConversion(name => name.Value, value => ListItemName.Create(value))
            .HasColumnName("name")
            .HasMaxLength(ListItemName.MaxLength)
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

        builder.Property(i => i.Importance)
            .HasColumnName("importance")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(i => i.DueDate)
            .HasColumnName("due_date");

        builder.Property(i => i.Reminder)
            .HasColumnName("reminder");

        builder.Property(i => i.Repeat)
            .HasColumnName("repeat")
            .HasMaxLength(100);

        builder.Property(i => i.ItemAreaId)
            .HasColumnName("item_area_id");

        builder.Property(i => i.TargetMemberId)
            .HasColumnName("target_member_id");

        // Shadow FK to parent list
        builder.Property<ListId>("ListId")
            .HasConversion(id => id.Value, value => ListId.From(value))
            .HasColumnName("shared_list_id")
            .IsRequired();
    }
}
