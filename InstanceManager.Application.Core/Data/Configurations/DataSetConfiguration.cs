using InstanceManager.Application.Core.Modules.DataSet;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstanceManager.Application.Core.Data.Configurations;

public class DataSetConfiguration : AuditableEntityConfiguration<DataSet>
{
    protected override void ConfigureEntity(EntityTypeBuilder<DataSet> builder)
    {
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Notes);

        // Store AllowedIdentityIds as JSON in SQLite
        builder.Property(e => e.AllowedIdentityIds)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            )
            .HasColumnType("TEXT");

        // Configure many-to-many relationship through DataSetInclude
        builder.HasMany(e => e.Includes)
            .WithOne(e => e.ParentDataSet)
            .HasForeignKey(e => e.ParentDataSetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.IncludedIn)
            .WithOne(e => e.IncludedDataSet)
            .HasForeignKey(e => e.IncludedDataSetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
