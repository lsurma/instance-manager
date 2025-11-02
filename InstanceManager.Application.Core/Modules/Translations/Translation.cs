using InstanceManager.Application.Core.Abstractions;

namespace InstanceManager.Application.Core.Modules.Translations;

public class Translation : AuditableEntityBase
{
    public required string InternalGroupName { get; set; }

    public required string ResourceName { get; set; }

    public required string TranslationName { get; set; }

    public required string CultureName { get; set; }

    public required string Content { get; set; }

    public Guid? DataSetId { get; set; }

    public DataSet.DataSet? DataSet { get; set; }
}
