namespace InstanceManager.Application.Contracts.ProjectInstance;

public class ProjectInstanceDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public string? MainHost { get; set; }

    public string? Notes { get; set; }

    public Guid? ParentProjectId { get; set; }

    public ProjectInstanceDto? ParentProject { get; set; }

    public ICollection<ProjectInstanceDto> ChildProjects { get; set; } = new List<ProjectInstanceDto>();

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public string CreatedBy { get; set; }
}
