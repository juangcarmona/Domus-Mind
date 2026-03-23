using DomusMind.Infrastructure.Initialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomusMind.Infrastructure.Persistence.Configurations.System;

public sealed class SystemInitializationConfiguration : IEntityTypeConfiguration<SystemInitializationRecord>
{
    public void Configure(EntityTypeBuilder<SystemInitializationRecord> builder)
    {
        builder.ToTable("system_initialization");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.InitializedAtUtc)
            .IsRequired();
    }
}
