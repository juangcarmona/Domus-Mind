using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.MealPlanning;

public sealed class MealSlotConfiguration : IEntityTypeConfiguration<MealSlot>
{
    public void Configure(EntityTypeBuilder<MealSlot> builder)
    {
        builder.ToTable("meal_slots");

        builder.HasKey(ms => ms.Id);

        builder.Property(ms => ms.Id)
            .HasConversion(id => id.Value, value => MealSlotId.From(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(ms => ms.MealPlanId)
            .HasConversion(id => id.Value, value => MealPlanId.From(value))
            .HasColumnName("meal_plan_id")
            .IsRequired();

        builder.Property(ms => ms.DayOfWeek)
            .HasConversion<string>()
            .HasColumnName("day_of_week")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ms => ms.MealType)
            .HasConversion<int>()
            .HasColumnName("meal_type")
            .IsRequired();

        builder.Property(ms => ms.MealSourceType)
            .HasConversion<int>()
            .HasColumnName("meal_source_type")
            .IsRequired();

        builder.Property(ms => ms.RecipeId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? RecipeId.From(value.Value) : (RecipeId?)null)
            .HasColumnName("recipe_id");

        builder.Property(ms => ms.FreeText)
            .HasColumnName("free_text")
            .HasMaxLength(500);

        builder.Property(ms => ms.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(ms => ms.IsOptional)
            .HasColumnName("is_optional")
            .IsRequired();

        builder.Property(ms => ms.IsLocked)
            .HasColumnName("is_locked")
            .IsRequired();

        builder.Property(ms => ms.AffectsWholeHousehold)
            .HasColumnName("affects_whole_household")
            .IsRequired();

        builder.Property(ms => ms.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(ms => ms.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.HasIndex(ms => new { ms.MealPlanId, ms.DayOfWeek, ms.MealType })
            .IsUnique()
            .HasDatabaseName("ix_meal_slots_plan_day_type");
    }
}
