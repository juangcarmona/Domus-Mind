using DomusMind.Domain.Family;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.MealPlanning;

public sealed class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("recipes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => new RecipeId(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(r => r.FamilyId)
            .HasConversion(id => id.Value, value => new FamilyId(value))
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(r => r.PrepTimeMinutes)
            .HasColumnName("prep_time_minutes");

        builder.Property(r => r.CookTimeMinutes)
            .HasColumnName("cook_time_minutes");

        builder.Ignore(r => r.TotalTimeMinutes);

        builder.Property(r => r.Servings)
            .HasColumnName("servings");

        builder.Property(r => r.IsFavorite)
            .HasColumnName("is_favorite")
            .IsRequired();

        builder.Property(r => r.AllowedMealTypes)
            .HasConversion(
                v => string.Join(",", v.Select(x => (int)x)),
                v => string.IsNullOrWhiteSpace(v)
                    ? new List<MealType>()
                    : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => (MealType)int.Parse(x))
                        .ToList())
            .HasColumnName("allowed_meal_types")
            .HasColumnType("text");

        builder.Property(r => r.Tags)
            .HasConversion(
                v => string.Join(",", v),
                v => string.IsNullOrWhiteSpace(v)
                    ? new List<string>()
                    : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasColumnName("tags")
            .HasColumnType("text");

        builder.Property(r => r.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedAtUtc)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(r => r.Ingredients)
            .WithOne()
            .HasForeignKey(i => i.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.Ingredients)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(r => new { r.FamilyId, r.Name })
            .IsUnique()
            .HasDatabaseName("ix_recipes_family_id_name");

        builder.Ignore(r => r.DomainEvents);
    }
}
