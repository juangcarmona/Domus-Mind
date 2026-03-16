using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.Family;

public sealed class FamilyConfiguration : IEntityTypeConfiguration<Domain.Family.Family>
{
    public void Configure(EntityTypeBuilder<Domain.Family.Family> builder)
    {
        builder.ToTable("families");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(f => f.Name)
            .HasConversion(
                name => name.Value,
                value => FamilyName.Create(value))
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(f => f.PrimaryLanguageCode)
            .HasColumnName("primary_language_code")
            .HasMaxLength(10);

        builder.Property(f => f.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.HasMany(f => f.Members)
            .WithOne()
            .HasForeignKey("FamilyId")
            .IsRequired();

        builder.Navigation(f => f.Members)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(f => f.DomainEvents);
    }
}
