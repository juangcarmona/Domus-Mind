using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.Family;

public sealed class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMember>
{
    public void Configure(EntityTypeBuilder<FamilyMember> builder)
    {
        builder.ToTable("family_members");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasConversion(
                id => id.Value,
                value => MemberId.From(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(m => m.Name)
            .HasConversion(
                name => name.Value,
                value => MemberName.Create(value))
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.Role)
            .HasConversion(
                role => role.Value,
                value => MemberRole.Create(value))
            .HasColumnName("role")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.JoinedAtUtc)
            .HasColumnName("joined_at_utc")
            .IsRequired();

        builder.Property(m => m.IsManager)
            .HasColumnName("is_manager")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(m => m.BirthDate)
            .HasColumnName("birth_date")
            .IsRequired(false);

        // Shadow FK to parent family
        builder.Property<FamilyId>("FamilyId")
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value))
            .HasColumnName("family_id")
            .IsRequired();
    }
}
