using MediatR;

namespace InstanceManager.Application.Contracts.Modules.DataSet;

public class SaveDataSetCommand : IRequest<Guid>
{
    public Guid? Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? Notes { get; set; }

    public ICollection<Guid> IncludedDataSetIds { get; set; } = new List<Guid>();
}
