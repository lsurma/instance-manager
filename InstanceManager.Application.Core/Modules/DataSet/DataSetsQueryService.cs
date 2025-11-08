using InstanceManager.Application.Core.Common;

namespace InstanceManager.Application.Core.Modules.DataSet;

/// <summary>
/// Query service for DataSet entities
/// </summary>
public class DataSetsQueryService : QueryService<DataSet, Guid>
{
    public DataSetsQueryService(IFilterHandlerRegistry filterHandlerRegistry)
        : base(filterHandlerRegistry)
    {
    }
}
