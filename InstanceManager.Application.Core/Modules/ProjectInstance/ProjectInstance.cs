using InstanceManager.Application.Core.Abstractions;

namespace InstanceManager.Application.Core.Modules.ProjectInstance;

public class ProjectInstance : AuditableEntityBase
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? MainHost { get; set; }

    public string? Notes { get; set; }

    public Guid? ParentProjectId { get; set; }

    public ProjectInstance? ParentProject { get; set; }

    public ICollection<ProjectInstance> ChildProjects { get; set; } = new List<ProjectInstance>();
}
