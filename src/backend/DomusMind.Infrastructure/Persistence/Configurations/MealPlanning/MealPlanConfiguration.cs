using DomusMind.Domain.Family;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.MealPlanning;

public sealed class MealPlanConfiguration : IEntityTypeConfiguration<MealPlan>
{
    public void Configure(EntityTypeBuilder<MealPlan> builder)
    {
        builder.ToTable("meal_plans");

        builder.HasKey(mp => mp.Id);

        builder.Property(mp => mp.Id)
            .HasConversion(id => id.Value, value => MealPlanId.From(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(mp => mp.FamilyId)
            .HasConversion(id => id.Value, value => FamilyId.From(value))
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(mp => mp.WeekStart)
            .HasColumnName("week_start")
            .IsRequired();

        builder.Property(mp => mp.Status)
            .HasConversion<int>()
            .HasColumnName("status")
            .IsRequired();

        builder.Property(mp => mp.AppliedTemplateId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? WeeklyTemplateId.From(value.Value) : (WeeklyTemplateId?)null)
            .HasColumnName("applied_template_id");

        builder.Property(mp => mp.ShoppingListId)
            .HasColumnName("shopping_list_id");

        builder.Property(mp => mp.ShoppingListVersion)
            .HasColumnName("shopping_list_version")
            .IsRequired();

        builder.Property(mp => mp.LastDerivedAt)
            .HasColumnName("last_derived_at");

        builder.Property(mp => mp.AffectsWholeHousehold)
            .HasColumnName("affects_whole_household")
            .IsRequired();

        builder.Property(mp => mp.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(mp => mp.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.Ignore(mp => mp.WeekEnd);

        builder.HasMany(mp => mp.MealSlots)
            .WithOne()
            .HasForeignKey(ms => ms.MealPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(mp => mp.MealSlots)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(mp => new { mp.FamilyId, mp.WeekStart })
            .IsUnique()
            .HasDatabaseName("ix_meal_plans_family_id_week_start");

        builder.Ignore(mp => mp.DomainEvents);
    }
}
