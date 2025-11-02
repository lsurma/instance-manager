using System.Linq.Expressions;
using InstanceManager.Application.Contracts.Modules.Translations;
using InstanceManager.Application.Core.Common;

namespace InstanceManager.Application.Core.Modules.Translations.Filters;

public class DataSetIdFilterHandler : IFilterHandler<Translation, DataSetIdFilter>
{
    public Expression<Func<Translation, bool>> GetFilterExpression(DataSetIdFilter filter)
    {
        var dataSetId = filter.Value!.Value; // We know it has value because IsActive() was checked
        return t => t.DataSetId == dataSetId;
    }
}

public class CultureNameFilterHandler : IFilterHandler<Translation, CultureNameFilter>
{
    public Expression<Func<Translation, bool>> GetFilterExpression(CultureNameFilter filter)
    {
        var cultureName = filter.Value!; // We know it has value because IsActive() was checked
        return t => t.CultureName == cultureName;
    }
}
