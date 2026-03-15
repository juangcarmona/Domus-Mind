using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.Family;

public sealed class UserFamilyAccessConfiguration : IEntityTypeConfiguration<UserFamilyAccess>
{
    public void Configure(EntityTypeBuilder<UserFamilyAccess> builder)
    {
        builder.ToTable("user_family_access");

        builder.HasKey(x => new { x.UserId, x.FamilyId });

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.FamilyId)
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(x => x.GrantedAtUtc)
            .HasColumnName("granted_at_utc")
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_user_family_access_user_id");
    }
}
