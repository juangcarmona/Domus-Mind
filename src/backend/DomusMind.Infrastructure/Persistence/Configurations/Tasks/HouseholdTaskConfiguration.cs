using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.Tasks;

public sealed class HouseholdTaskConfiguration : IEntityTypeConfiguration<HouseholdTask>
{
    public void Configure(EntityTypeBuilder<HouseholdTask> builder)
    {
        builder.ToTable("household_tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => TaskId.From(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(t => t.FamilyId)
            .HasConversion(id => id.Value, value => FamilyId.From(value))
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(t => t.Title)
            .HasConversion(title => title.Value, value => TaskTitle.Create(value))
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(t => t.DueDate)
            .HasColumnName("due_date");

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.AssigneeId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? MemberId.From(value.Value) : null)
            .HasColumnName("assignee_id");

        builder.Property(t => t.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Ignore(t => t.DomainEvents);
    }
}
