using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.MealPlanning;

public sealed class MealSlotTemplateConfiguration : IEntityTypeConfiguration<MealSlotTemplate>
{
    public void Configure(EntityTypeBuilder<MealSlotTemplate> builder)
    {
        builder.ToTable("meal_slot_templates");

        builder.HasKey(mst => mst.Id);

        builder.Property(mst => mst.Id)
            .HasConversion(id => id.Value, value => MealSlotTemplateId.From(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(mst => mst.WeeklyTemplateId)
            .HasConversion(id => id.Value, value => WeeklyTemplateId.From(value))
            .HasColumnName("weekly_template_id")
            .IsRequired();

        builder.Property(mst => mst.DayOfWeek)
            .HasConversion<string>()
            .HasColumnName("day_of_week")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(mst => mst.MealType)
            .HasConversion<int>()
            .HasColumnName("meal_type")
            .IsRequired();

        builder.Property(mst => mst.MealSourceType)
            .HasConversion<int>()
            .HasColumnName("meal_source_type")
            .IsRequired();

        builder.Property(mst => mst.RecipeId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? RecipeId.From(value.Value) : (RecipeId?)null)
            .HasColumnName("recipe_id");

        builder.Property(mst => mst.FreeText)
            .HasColumnName("free_text")
            .HasMaxLength(500);

        builder.Property(mst => mst.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(mst => mst.IsOptional)
            .HasColumnName("is_optional")
            .IsRequired();

        builder.Property(mst => mst.IsLocked)
            .HasColumnName("is_locked")
            .IsRequired();

        builder.Property(mst => mst.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(mst => mst.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.HasIndex(mst => new { mst.WeeklyTemplateId, mst.DayOfWeek, mst.MealType })
            .IsUnique()
            .HasDatabaseName("ix_meal_slot_templates_template_day_type");
    }
}
