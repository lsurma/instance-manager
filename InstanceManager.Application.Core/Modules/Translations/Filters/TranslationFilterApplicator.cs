using System.Linq.Expressions;
using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Contracts.Modules.Translations;
using InstanceManager.Application.Core.Common;

namespace InstanceManager.Application.Core.Modules.Translations.Filters;

public class DataSetIdFilterHandler : IFilterHandler<Translation, DataSetIdFilter>
{
    public Task<Expression<Func<Translation, bool>>> GetFilterExpressionAsync(DataSetIdFilter filter, CancellationToken cancellationToken = default)
    {
        var dataSetId = filter.Value!.Value; // We know it has value because IsActive() was checked
        Expression<Func<Translation, bool>> expression = t => t.DataSetId == dataSetId;
        return Task.FromResult(expression);
    }
}

public class CultureNameFilterHandler : IFilterHandler<Translation, CultureNameFilter>
{
    public Task<Expression<Func<Translation, bool>>> GetFilterExpressionAsync(CultureNameFilter filter, CancellationToken cancellationToken = default)
    {
        var cultureName = filter.Value!; // We know it has value because IsActive() was checked
        Expression<Func<Translation, bool>> expression = t => t.CultureName == cultureName;
        return Task.FromResult(expression);
    }
}

public class TranslationSearchFilterHandler : IFilterHandler<Translation, SearchFilter>
{
    public Task<Expression<Func<Translation, bool>>> GetFilterExpressionAsync(SearchFilter filter, CancellationToken cancellationToken = default)
    {
        var searchTerm = filter.SearchTerm!.ToLower(); // We know it has value because IsActive() was checked
        Expression<Func<Translation, bool>> expression = t =>
            t.InternalGroupName.ToLower().Contains(searchTerm) ||
            t.ResourceName.ToLower().Contains(searchTerm) ||
            t.TranslationName.ToLower().Contains(searchTerm) ||
            t.Content.ToLower().Contains(searchTerm);
        return Task.FromResult(expression);
    }
}
