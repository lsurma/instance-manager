using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstanceManager.Application.Core.Data.Configurations;

public class DataSetConfiguration : AuditableEntityConfiguration<Modules.DataSet.DataSet>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Modules.DataSet.DataSet> builder)
    {
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Notes);

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
