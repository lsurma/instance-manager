using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstanceManager.Application.Core.Data.Configurations;

public class TranslationConfiguration : IEntityTypeConfiguration<Modules.Translations.Translation>
{
    public void Configure(EntityTypeBuilder<Modules.Translations.Translation> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.InternalGroupName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ResourceName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.TranslationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.CultureName)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(e => e.Content)
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);
        
        // SQLite workaround: Store DateTimeOffset as UTC ticks for proper sorting/filtering
        builder.Property(e => e.CreatedAt)
            .HasConversion(
                v => v.UtcDateTime.Ticks,
                v => new DateTimeOffset(v, TimeSpan.Zero));
        
        builder.Property(e => e.UpdatedAt)
            .HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime.Ticks : (long?)null,
                v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null);

        // Configure relationship with DataSet
        builder.HasOne(e => e.DataSet)
            .WithMany()
            .HasForeignKey(e => e.DataSetId)
            .OnDelete(DeleteBehavior.SetNull);

        // Add indexes for common queries
        builder.HasIndex(e => e.DataSetId);
        builder.HasIndex(e => e.CultureName);
        builder.HasIndex(e => new { e.InternalGroupName, e.ResourceName, e.CultureName });
    }
}
