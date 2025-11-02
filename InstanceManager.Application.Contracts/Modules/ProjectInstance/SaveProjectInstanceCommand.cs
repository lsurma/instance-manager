using MediatR;

namespace InstanceManager.Application.Contracts.Modules.ProjectInstance;

public class SaveProjectInstanceCommand : IRequest<Guid>
{
    public Guid? Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? MainHost { get; set; }

    public string? Notes { get; set; }

    public Guid? ParentProjectId { get; set; }
}
