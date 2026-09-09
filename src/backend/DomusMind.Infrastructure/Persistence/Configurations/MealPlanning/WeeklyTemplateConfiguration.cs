using DomusMind.Domain.Family;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.MealPlanning;

public sealed class WeeklyTemplateConfiguration : IEntityTypeConfiguration<WeeklyTemplate>
{
    public void Configure(EntityTypeBuilder<WeeklyTemplate> builder)
    {
        builder.ToTable("weekly_templates");

        builder.HasKey(wt => wt.Id);

        builder.Property(wt => wt.Id)
            .HasConversion(id => id.Value, value => new WeeklyTemplateId(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(wt => wt.FamilyId)
            .HasConversion(id => id.Value, value => new FamilyId(value))
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(wt => wt.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(wt => wt.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(wt => wt.UpdatedAtUtc)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(wt => wt.MealSlotTemplates)
            .WithOne()
            .HasForeignKey(mst => mst.WeeklyTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(wt => wt.MealSlotTemplates)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(wt => new { wt.FamilyId, wt.Name })
            .IsUnique()
            .HasDatabaseName("ix_weekly_templates_family_id_name");

        builder.Ignore(wt => wt.DomainEvents);
    }
}