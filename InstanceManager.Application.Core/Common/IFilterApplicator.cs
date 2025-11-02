using System.Linq.Expressions;
using InstanceManager.Application.Contracts.Common;

namespace InstanceManager.Application.Core.Common;

/// <summary>
/// Handler for applying a specific filter to entity queries.
/// Each filter type has its own handler implementation.
/// Supports async operations for filters that require database lookups or other async operations.
/// </summary>
public interface IFilterHandler<TEntity, TFilter> where TFilter : IQueryFilter
{
    Task<Expression<Func<TEntity, bool>>> GetFilterExpressionAsync(TFilter filter, CancellationToken cancellationToken = default);
}
