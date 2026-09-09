using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.MealPlanning;

public sealed class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.ToTable("ingredients");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasConversion(id => id.Value, value => new IngredientId(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(i => i.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .HasColumnName("quantity");

        builder.Property(i => i.Unit)
            .HasColumnName("unit")
            .HasMaxLength(50);

        builder.Property(i => i.RecipeId)
            .HasConversion(id => id.Value, value => new RecipeId(value))
            .HasColumnName("recipe_id")
            .IsRequired();

        builder.Property(i => i.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(i => new { i.RecipeId, i.Name })
            .IsUnique()
            .HasDatabaseName("ix_ingredients_recipe_id_name");
    }
}