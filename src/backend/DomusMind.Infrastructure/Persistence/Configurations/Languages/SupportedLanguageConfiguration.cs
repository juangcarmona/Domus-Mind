using DomusMind.Infrastructure.Languages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.Languages;

public sealed class SupportedLanguageConfiguration : IEntityTypeConfiguration<SupportedLanguage>
{
    public void Configure(EntityTypeBuilder<SupportedLanguage> builder)
    {
        builder.ToTable("supported_languages");

        builder.HasKey(x => x.Code);

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Culture)
            .HasColumnName("culture")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.NativeDisplayName)
            .HasColumnName("native_display_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IsDefault)
            .HasColumnName("is_default")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.HasIndex(x => x.IsDefault)
            .HasDatabaseName("ix_supported_languages_is_default");
    }
}
