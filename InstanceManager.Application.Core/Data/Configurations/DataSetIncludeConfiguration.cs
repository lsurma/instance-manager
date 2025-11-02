using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstanceManager.Application.Core.Data.Configurations;

public class DataSetIncludeConfiguration : IEntityTypeConfiguration<Modules.DataSet.DataSetInclude>
{
    public void Configure(EntityTypeBuilder<Modules.DataSet.DataSetInclude> builder)
    {
        builder.HasKey(e => new { e.ParentDataSetId, e.IncludedDataSetId });

        builder.Property(e => e.CreatedAt)
            .HasConversion(
                v => v.UtcDateTime.Ticks,
                v => new DateTimeOffset(v, TimeSpan.Zero));

        // Prevent circular references (a dataset cannot include itself)
        builder.HasIndex(e => new { e.ParentDataSetId, e.IncludedDataSetId })
            .IsUnique();
    }
}
