using InstanceManager.Application.Core.Abstractions;

namespace InstanceManager.Application.Core.Modules.DataSet;

public class DataSet : AuditableEntityBase
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? Notes { get; set; }

    public ICollection<DataSetInclude> Includes { get; set; } = new List<DataSetInclude>();

    public ICollection<DataSetInclude> IncludedIn { get; set; } = new List<DataSetInclude>();
}
