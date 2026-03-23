using DomusMind.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.Auth;

public sealed class AuthUserConfiguration : IEntityTypeConfiguration<AuthUser>
{
    public void Configure(EntityTypeBuilder<AuthUser> builder)
    {
        builder.ToTable("auth_users");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.MustChangePassword)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.DisplayName)
            .HasMaxLength(150);

        builder.Property(x => x.IsDisabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.PasswordChangedAtUtc);

        builder.Property(x => x.MemberId);

        builder.Property(x => x.CreatedByUserId);

        builder.Property(x => x.LastLoginAtUtc);
    }
}
