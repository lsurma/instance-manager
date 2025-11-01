namespace InstanceManager.Application.Core.DataSet;

public class DataSetInclude
{
    public Guid ParentDataSetId { get; set; }
    public DataSet ParentDataSet { get; set; } = null!;

    public Guid IncludedDataSetId { get; set; }
    public DataSet IncludedDataSet { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
}
