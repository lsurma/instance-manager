using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstanceManager.Application.Core.Data.Configurations;

public class TranslationConfiguration : AuditableEntityConfiguration<Modules.Translations.Translation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Modules.Translations.Translation> builder)
    {
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
