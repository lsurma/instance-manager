using MediatR;

namespace InstanceManager.Application.Contracts.Modules.DataSet;

public class SaveDataSetCommand : IRequest<Guid>
{
    public Guid? Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// List of user/application identity IDs that have access to this DataSet.
    /// If empty, the DataSet has public access (no restrictions).
    /// </summary>
    public ICollection<string> AllowedIdentityIds { get; set; } = new List<string>();

    public ICollection<Guid> IncludedDataSetIds { get; set; } = new List<Guid>();
}
