namespace InstanceManager.Application.Core.Modules.Translations;

public class Translation
{
    public Guid Id { get; set; }

    public required string InternalGroupName { get; set; }

    public required string ResourceName { get; set; }

    public required string TranslationName { get; set; }

    public required string CultureName { get; set; }

    public required string Content { get; set; }

    public Guid? DataSetId { get; set; }

    public DataSet.DataSet? DataSet { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public required string CreatedBy { get; set; }
}
