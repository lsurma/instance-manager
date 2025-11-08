using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Core.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Common;

/// <summary>
/// Base marker interface for query services
/// </summary>
public interface IQueryService
{
}

/// <summary>
/// Generic query service interface for a specific entity type with primary key
/// </summary>
public interface IQueryService<TEntity, TPrimaryKey> : IQueryService
    where TEntity : class, IEntity<TPrimaryKey>
    where TPrimaryKey : notnull
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
        QueryOptions<TEntity, TPrimaryKey>? options = null,
        CancellationToken cancellationToken = default);

    Task<PaginatedList<TDto>> ExecutePaginatedQueryAsync<TDto>(
        IQueryable<TEntity> query,
        PaginationParameters pagination,
        Func<List<TEntity>, List<TDto>> mapper,
        CancellationToken cancellationToken = default);

    Task<PaginatedList<TItem>> ExecutePaginatedQueryAsync<TItem>(
        IQueryable<TEntity> query,
        PaginationParameters pagination,
        QueryOptions<TEntity, TPrimaryKey, TItem>? options = null,
        CancellationToken cancellationToken = default);

    // ID-based helper methods
    IQueryable<TEntity> FilterById(IQueryable<TEntity> query, TPrimaryKey id);
    
    IQueryable<TEntity> FilterByIds(IQueryable<TEntity> query, IEnumerable<TPrimaryKey> ids);
    

    Task<TEntity?> GetByIdAsync(IQueryable<TEntity> query, TPrimaryKey id, QueryOptions<TEntity, TPrimaryKey>? options = null, CancellationToken cancellationToken = default);
    
    Task<TResult?> GetByIdAsync<TResult>(IQueryable<TEntity> query, TPrimaryKey id, QueryOptions<TEntity, TPrimaryKey, TResult>? options = null, CancellationToken cancellationToken = default);

    // Batch operations
    Task<List<TEntity>> GetByIdsAsync(IQueryable<TEntity> query, IEnumerable<TPrimaryKey> ids, QueryOptions<TEntity, TPrimaryKey>? options = null, CancellationToken cancellationToken = default);
    
    Task<List<TResult>> GetByIdsAsync<TResult>(IQueryable<TEntity> query, IEnumerable<TPrimaryKey> ids, QueryOptions<TEntity, TPrimaryKey, TResult>? options = null, CancellationToken cancellationToken = default);

    // Existence checks
    Task<bool> ExistsAsync(IQueryable<TEntity> query, TPrimaryKey id, QueryOptions<TEntity, TPrimaryKey>? options = null, CancellationToken cancellationToken = default);
    
    Task<bool> AnyAsync(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    // Count operations
    Task<int> CountAsync(IQueryable<TEntity> query, QueryOptions<TEntity, TPrimaryKey>? options = null, CancellationToken cancellationToken = default);

    // First/Single operations
    Task<TEntity?> FirstOrDefaultAsync(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate, QueryOptions<TEntity, TPrimaryKey>? options = null, CancellationToken cancellationToken = default);
    
    Task<TResult?> FirstOrDefaultAsync<TResult>(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate, QueryOptions<TEntity, TPrimaryKey, TResult>? options = null, CancellationToken cancellationToken = default);
    
    Task<TEntity> SingleAsync(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate, QueryOptions<TEntity, TPrimaryKey>? options = null, CancellationToken cancellationToken = default);
    
    Task<TResult> SingleAsync<TResult>(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate, QueryOptions<TEntity, TPrimaryKey, TResult>? options = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Abstract base query service implementation for a specific entity type with primary key.
/// Use this to create entity-specific query services like TranslationsQueryService by inheriting from this class.
/// </summary>
public abstract class QueryService<TEntity, TPrimaryKey> : IQueryService<TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
    where TPrimaryKey : notnull
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
    /// Prepares a query by applying filters, includes, and ordering (but not pagination).
    /// All configuration is provided through QueryOptions.
    /// </summary>
    public virtual async Task<IQueryable<TEntity>> PrepareQueryAsync(
        IQueryable<TEntity> query,
        QueryOptions<TEntity, TPrimaryKey>? options = null,
        CancellationToken cancellationToken = default)
    {
        if (options == null)
        {
            return query;
        }

        // Apply AsNoTracking if requested
        if (options.AsNoTracking)
        {
            query = query.AsNoTracking();
        }

        // Apply filters
        if (options.Filtering?.HasQueryFilters() == true)
        {
            var filterHandlers = _filterHandlerRegistry.GetHandlersForEntity<TEntity>();

            foreach (var filter in options.Filtering.QueryFilters.Where(f => f.IsActive()))
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

        // Apply type-safe includes
        foreach (var include in options.Includes)
        {
            query = query.Include(include);
        }

        // Apply IncludeFunc if provided (for backward compatibility or complex scenarios)
        if (options.IncludeFunc != null)
        {
            query = options.IncludeFunc(query);
        }

        // Apply ordering if provided
        if (options.Ordering != null)
        {
            query = ApplyOrdering(query, options.Ordering);
        }

        return query;
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

    /// <summary>
    /// Filters a query to return only the entity with the specified ID
    /// </summary>
    public IQueryable<TEntity> FilterById(IQueryable<TEntity> query, TPrimaryKey id)
    {
        return query.Where(e => e.Id.Equals(id));
    }

    /// <summary>
    /// Filters a query to return only entities with IDs in the specified collection
    /// </summary>
    public IQueryable<TEntity> FilterByIds(IQueryable<TEntity> query, IEnumerable<TPrimaryKey> ids)
    {
        return query.Where(e => ids.Contains(e.Id));
    }

    /// <summary>
    /// Gets a single entity by ID, with optional query preparation (authorization, includes, filters, etc.).
    /// The query is prepared using PrepareQueryAsync before filtering by ID.
    /// </summary>
    public async Task<TEntity?> GetByIdAsync(
        IQueryable<TEntity> query,
        TPrimaryKey id,
        QueryOptions<TEntity, TPrimaryKey>? options = null,
        CancellationToken cancellationToken = default)
    {
        // Apply authorization, filters, and includes via PrepareQueryAsync
        query = await PrepareQueryAsync(query, options, cancellationToken);

        // Filter by ID and fetch
        return await FilterById(query, id).FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a single entity by ID with optional projection, with optional query preparation (authorization, includes, filters, etc.).
    /// If options.Selector is null, returns the full entity cast to TResult (TResult should be TEntity in this case).
    /// If options.Selector is provided, it's applied at the database level for efficient querying.
    /// </summary>
    public async Task<TResult?> GetByIdAsync<TResult>(
        IQueryable<TEntity> query,
        TPrimaryKey id,
        QueryOptions<TEntity, TPrimaryKey, TResult>? options = null,
        CancellationToken cancellationToken = default)
    {
        // Apply authorization, filters, and includes via PrepareQueryAsync
        query = await PrepareQueryAsync(query, options, cancellationToken);

        // Filter by ID
        var filteredQuery = FilterById(query, id);

        // Apply selector if provided in options, otherwise return entity as TResult
        if (options?.Selector != null)
        {
            return await filteredQuery.Select(options.Selector).FirstOrDefaultAsync(cancellationToken);
        }
        else
        {
            // No projection - return full entity cast to TResult
            var entity = await filteredQuery.FirstOrDefaultAsync(cancellationToken);
            return (TResult?)(object?)entity;
        }
    }

    /// <summary>
    /// Gets multiple entities by IDs with optional query preparation (authorization, includes, filters, etc.).
    /// </summary>
    public async Task<List<TEntity>> GetByIdsAsync(
        IQueryable<TEntity> query,
        IEnumerable<TPrimaryKey> ids,
        QueryOptions<TEntity, TPrimaryKey>? options = null,
        CancellationToken cancellationToken = default)
    {
        // Apply authorization, filters, and includes via PrepareQueryAsync
        query = await PrepareQueryAsync(query, options, cancellationToken);

        // Filter by IDs and fetch
        return await FilterByIds(query, ids).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets multiple entities by IDs with optional projection and query preparation.
    /// If options.Selector is null, returns full entities cast to TResult.
    /// If options.Selector is provided, it's applied at the database level for efficient querying.
    /// </summary>
    public async Task<List<TResult>> GetByIdsAsync<TResult>(
        IQueryable<TEntity> query,
        IEnumerable<TPrimaryKey> ids,
        QueryOptions<TEntity, TPrimaryKey, TResult>? options = null,
        CancellationToken cancellationToken = default)
    {
        // Apply authorization, filters, and includes via PrepareQueryAsync
        query = await PrepareQueryAsync(query, options, cancellationToken);

        // Filter by IDs
        var filteredQuery = FilterByIds(query, ids);

        // Apply selector if provided in options, otherwise return entities as TResult
        if (options?.Selector != null)
        {
            return await filteredQuery.Select(options.Selector).ToListAsync(cancellationToken);
        }
        else
        {
            // No projection - return full entities cast to TResult
            var entities = await filteredQuery.ToListAsync(cancellationToken);
            return entities.Cast<TResult>().ToList();
        }
    }

    /// <summary>
    /// Checks if an entity with the specified ID exists, with optional query preparation (authorization, filters, etc.).
    /// </summary>
    public async Task<bool> ExistsAsync(
        IQueryable<TEntity> query,
        TPrimaryKey id,
        QueryOptions<TEntity, TPrimaryKey>? options = null,
        CancellationToken cancellationToken = default)
    {
        // Apply authorization, filters, and includes via PrepareQueryAsync
        query = await PrepareQueryAsync(query, options, cancellationToken);

        // Check existence by ID
        return await FilterById(query, id).AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if any entities match the specified predicate.
    /// </summary>
    public async Task<bool> AnyAsync(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await query.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Gets the count of entities with optional query preparation (authorization, filters, etc.).
    /// </summary>
    public async Task<int> CountAsync(
        IQueryable<TEntity> query,
        QueryOptions<TEntity, TPrimaryKey>? options = null,
        CancellationToken cancellationToken = default)
    {
        // Apply authorization, filters, and includes via PrepareQueryAsync
        query = await PrepareQueryAsync(query, options, cancellationToken);

        return await query.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the first entity matching the predicate, or null if not found, with optional query preparation.
    /// </summary>
    public async Task<TEntity?> FirstOrDefaultAsync(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, bool>> predicate,
        QueryOptions<TEntity, TPrimaryKey>? options = null,
        CancellationToken cancellationToken = default)
    {
        // Apply authorization, filters, and includes via PrepareQueryAsync
        query = await PrepareQueryAsync(query, options, cancellationToken);

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Gets the first entity matching the predicate with optional projection, or null if not found.
    /// If options.Selector is null, returns the full entity cast to TResult.
    /// If options.Selector is provided, it's applied at the database level for efficient querying.
    /// </summary>
    public async Task<TResult?> FirstOrDefaultAsync<TResult>(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, bool>> predicate,
        QueryOptions<TEntity, TPrimaryKey, TResult>? options = null,
        CancellationToken cancellationToken = default)
    {
        // Apply authorization, filters, and includes via PrepareQueryAsync
        query = await PrepareQueryAsync(query, options, cancellationToken);

        // Apply predicate
        var filteredQuery = query.Where(predicate);

        // Apply selector if provided in options, otherwise return entity as TResult
        if (options?.Selector != null)
        {
            return await filteredQuery.Select(options.Selector).FirstOrDefaultAsync(cancellationToken);
        }
        else
        {
            // No projection - return full entity cast to TResult
            var entity = await filteredQuery.FirstOrDefaultAsync(cancellationToken);
            return (TResult?)(object?)entity;
        }
    }

    /// <summary>
    /// Gets the single entity matching the predicate, with optional query preparation.
    /// Throws if zero or more than one entity is found.
    /// </summary>
    public async Task<TEntity> SingleAsync(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, bool>> predicate,
        QueryOptions<TEntity, TPrimaryKey>? options = null,
        CancellationToken cancellationToken = default)
    {
        // Apply authorization, filters, and includes via PrepareQueryAsync
        query = await PrepareQueryAsync(query, options, cancellationToken);

        return await query.SingleAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Gets the single entity matching the predicate with optional projection.
    /// Throws if zero or more than one entity is found.
    /// If options.Selector is null, returns the full entity cast to TResult.
    /// If options.Selector is provided, it's applied at the database level for efficient querying.
    /// </summary>
    public async Task<TResult> SingleAsync<TResult>(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, bool>> predicate,
        QueryOptions<TEntity, TPrimaryKey, TResult>? options = null,
        CancellationToken cancellationToken = default)
    {
        // Apply authorization, filters, and includes via PrepareQueryAsync
        query = await PrepareQueryAsync(query, options, cancellationToken);

        // Apply predicate
        var filteredQuery = query.Where(predicate);

        // Apply selector if provided in options, otherwise return entity as TResult
        if (options?.Selector != null)
        {
            return await filteredQuery.Select(options.Selector).SingleAsync(cancellationToken);
        }
        else
        {
            // No projection - return full entity cast to TResult
            var entity = await filteredQuery.SingleAsync(cancellationToken);
            return (TResult)(object)entity;
        }
    }

    /// <summary>
    /// Executes a prepared query with optional projection and returns paginated results.
    /// If options.Selector is null, returns full entities cast to TItem (TItem should be TEntity in this case).
    /// If options.Selector is provided, it's applied at the database level for efficient querying.
    /// </summary>
    public async Task<PaginatedList<TItem>> ExecutePaginatedQueryAsync<TItem>(
        IQueryable<TEntity> query,
        PaginationParameters pagination,
        QueryOptions<TEntity, TPrimaryKey, TItem>? options = null,
        CancellationToken cancellationToken = default)
    {
        // Get total count before pagination
        var totalItems = await query.CountAsync(cancellationToken);

        // Apply pagination
        var paginatedQuery = ApplyPagination(query, pagination);

        // Apply selector if provided in options, otherwise cast entities to TItem
        List<TItem> dtos;
        if (options?.Selector != null)
        {
            dtos = await paginatedQuery.Select(options.Selector).ToListAsync(cancellationToken);
        }
        else
        {
            // No projection - return full entities cast to TItem
            var entities = await paginatedQuery.ToListAsync(cancellationToken);
            dtos = entities.Cast<TItem>().ToList();
        }

        return new PaginatedList<TItem>(dtos, totalItems, pagination.PageNumber, pagination.PageSize);
    }
}
