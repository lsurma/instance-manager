using InstanceManager.Application.Core.Abstractions;

namespace InstanceManager.Application.Core.Modules.DataSet;

public class DataSet : AuditableEntityBase
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// List of user/application identity IDs that have access to this DataSet.
    /// If empty or null, no access restrictions are applied (public access).
    /// </summary>
    public ICollection<string> AllowedIdentityIds { get; set; } = new List<string>();

    public ICollection<DataSetInclude> Includes { get; set; } = new List<DataSetInclude>();

    public ICollection<DataSetInclude> IncludedIn { get; set; } = new List<DataSetInclude>();
}
