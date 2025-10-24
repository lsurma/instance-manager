using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstanceManager.Application.Core.Data.Configurations;

public class ProjectInstanceConfiguration : IEntityTypeConfiguration<ProjectInstance.ProjectInstance>
{
    public void Configure(EntityTypeBuilder<ProjectInstance.ProjectInstance> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.MainHost)
            .HasMaxLength(500);

        builder.Property(e => e.Notes);

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(e => e.ParentProject)
            .WithMany(e => e.ChildProjects)
            .HasForeignKey(e => e.ParentProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ParentProjectId);
    }
}
