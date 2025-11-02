using System.Linq.Expressions;
using InstanceManager.Application.Contracts.Common;

namespace InstanceManager.Application.Core.Common;

/// <summary>
/// Handler for applying a specific filter to entity queries.
/// Each filter type has its own handler implementation.
/// </summary>
public interface IFilterHandler<TEntity, TFilter> where TFilter : IQueryFilter
{
    Expression<Func<TEntity, bool>> GetFilterExpression(TFilter filter);
}
