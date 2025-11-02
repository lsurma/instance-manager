using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using InstanceManager.Application.Contracts.Common;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Common;

/// <summary>
/// Base marker interface for query services
/// </summary>
public interface IQueryService
{
}

/// <summary>
/// Generic query service interface for a specific entity type
/// </summary>
public interface IQueryService<TEntity> : IQueryService where TEntity : class
{
    IQueryable<TEntity> ApplyOrdering(IQueryable<TEntity> query, OrderingParameters ordering);
    IQueryable<TEntity> ApplyPagination(IQueryable<TEntity> query, PaginationParameters pagination);
    Task<PaginatedList<TDto>> ToPaginatedListAsync<TDto>(
        IQueryable<TEntity> query,
        PaginationParameters pagination,
        Func<List<TEntity>, List<TDto>> mapper,
        CancellationToken cancellationToken = default);
    Task<IQueryable<TEntity>> PrepareQueryAsync(
        IQueryable<TEntity> query,
        FilteringParameters filtering,
        OrderingParameters ordering,
        QueryOptions<TEntity>? options = null,
        CancellationToken cancellationToken = default);

    Task<PaginatedList<TDto>> ExecutePaginatedQueryAsync<TDto>(
        IQueryable<TEntity> query,
        PaginationParameters pagination,
        Func<List<TEntity>, List<TDto>> mapper,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Generic query service implementation for a specific entity type.
/// Use this to create entity-specific query services like TranslationsQueryService.
/// </summary>
public class QueryService<TEntity> : IQueryService<TEntity> where TEntity : class
{
    private readonly IFilterHandlerRegistry _filterHandlerRegistry;

    public QueryService(IFilterHandlerRegistry filterHandlerRegistry)
    {
        _filterHandlerRegistry = filterHandlerRegistry;
    }

    /// <summary>
    /// Applies ordering to a queryable if ordering parameters are specified
    /// </summary>
    public IQueryable<TEntity> ApplyOrdering(IQueryable<TEntity> query, OrderingParameters ordering)
    {
        if (ordering.HasOrdering())
        {
            query = query.OrderBy($"{ordering.OrderBy} {ordering.GetOrderDirection()}");
        }

        return query;
    }

    /// <summary>
    /// Applies pagination (skip and take) to a queryable
    /// </summary>
    public IQueryable<TEntity> ApplyPagination(IQueryable<TEntity> query, PaginationParameters pagination)
    {
        return query
            .Skip(pagination.Skip)
            .Take(pagination.PageSize);
    }

    /// <summary>
    /// Executes a paginated query and returns a PaginatedList
    /// </summary>
    public async Task<PaginatedList<TDto>> ToPaginatedListAsync<TDto>(
        IQueryable<TEntity> query,
        PaginationParameters pagination,
        Func<List<TEntity>, List<TDto>> mapper,
        CancellationToken cancellationToken = default)
    {
        // Get total count before pagination
        var totalItems = await query.CountAsync(cancellationToken);

        // Apply pagination and fetch data
        var entities = await ApplyPagination(query, pagination)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var dtos = mapper(entities);

        return new PaginatedList<TDto>(dtos, totalItems, pagination.PageNumber, pagination.PageSize);
    }

    /// <summary>
    /// Prepares a query by applying filters, includes, and ordering (but not pagination)
    /// </summary>
    public async Task<IQueryable<TEntity>> PrepareQueryAsync(
        IQueryable<TEntity> query,
        FilteringParameters filtering,
        OrderingParameters ordering,
        QueryOptions<TEntity>? options = null,
        CancellationToken cancellationToken = default)
    {
        // Apply filters
        if (filtering.HasQueryFilters())
        {
            var filterHandlers = _filterHandlerRegistry.GetHandlersForEntity<TEntity>();

            foreach (var filter in filtering.QueryFilters.Where(f => f.IsActive()))
            {
                var filterType = filter.GetType();
                if (!filterHandlers.TryGetValue(filterType, out var handler))
                {
                    continue;
                }

                // Get the GetFilterExpressionAsync method via reflection
                var handlerType = handler.GetType();
                var method = handlerType.GetMethod("GetFilterExpressionAsync");
                if (method == null)
                {
                    continue;
                }

                // Invoke the async method
                var task = method.Invoke(handler, new object[] { filter, cancellationToken });
                if (task is Task<Expression<Func<TEntity, bool>>> expressionTask)
                {
                    var expression = await expressionTask;
                    query = query.Where(expression);
                }
            }
        }

        if (options != null)
        {
            // Apply includes if provided
            if (options.IncludeFunc != null)
            {
                query = options.IncludeFunc(query);
            }
        }

        // Apply ordering
        query = ApplyOrdering(query, ordering);

        return await Task.FromResult(query);
    }

    /// <summary>
    /// Executes a prepared query and returns paginated results
    /// </summary>
    public async Task<PaginatedList<TDto>> ExecutePaginatedQueryAsync<TDto>(
        IQueryable<TEntity> query,
        PaginationParameters pagination,
        Func<List<TEntity>, List<TDto>> mapper,
        CancellationToken cancellationToken = default)
    {
        return await ToPaginatedListAsync(query, pagination, mapper, cancellationToken);
    }
}
