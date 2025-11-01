namespace InstanceManager.Application.Core.DataSet;

public class DataSet
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? Notes { get; set; }

    public ICollection<DataSetInclude> Includes { get; set; } = new List<DataSetInclude>();

    public ICollection<DataSetInclude> IncludedIn { get; set; } = new List<DataSetInclude>();

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public required string CreatedBy { get; set; }
}
