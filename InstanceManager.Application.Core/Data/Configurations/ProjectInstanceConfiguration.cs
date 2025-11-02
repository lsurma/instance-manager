using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstanceManager.Application.Core.Data.Configurations;

public class ProjectInstanceConfiguration : AuditableEntityConfiguration<Modules.ProjectInstance.ProjectInstance>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Modules.ProjectInstance.ProjectInstance> builder)
    {
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.MainHost)
            .HasMaxLength(500);

        builder.Property(e => e.Notes);

        builder.HasOne(e => e.ParentProject)
            .WithMany(e => e.ChildProjects)
            .HasForeignKey(e => e.ParentProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ParentProjectId);
    }
}
