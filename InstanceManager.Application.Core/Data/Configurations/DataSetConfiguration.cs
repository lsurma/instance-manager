using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstanceManager.Application.Core.Data.Configurations;

public class DataSetConfiguration : IEntityTypeConfiguration<Modules.DataSet.DataSet>
{
    public void Configure(EntityTypeBuilder<Modules.DataSet.DataSet> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Notes);

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
