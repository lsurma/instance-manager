using System.Linq.Dynamic.Core;
using InstanceManager.Application.Contracts.Common;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Common;

public interface IQueryService
{
    IQueryable<T> ApplyOrdering<T>(IQueryable<T> query, OrderingParameters ordering);
    IQueryable<T> ApplyPagination<T>(IQueryable<T> query, PaginationParameters pagination);
    Task<PaginatedList<TDto>> ToPaginatedListAsync<TEntity, TDto>(
        IQueryable<TEntity> query,
        PaginationParameters pagination,
        Func<List<TEntity>, List<TDto>> mapper,
        CancellationToken cancellationToken = default);
    IQueryable<T> ApplyTextSearch<T>(
        IQueryable<T> query,
        FilteringParameters filtering,
        Func<string, IQueryable<T>, IQueryable<T>> searchPredicate);
    
    IQueryable<TEntity> ApplySpecification<TEntity>(
        IQueryable<TEntity> query,
        FilteringParameters filtering,
        Func<string, IBasicSpecification<TEntity>> specificationFactory);
    IQueryable<TEntity> PrepareQuery<TEntity>(
        IQueryable<TEntity> query,
        FilteringParameters filtering,
        OrderingParameters ordering,
        QueryOptions<TEntity>? options = null);
    
    Task<PaginatedList<TDto>> ExecutePaginatedQueryAsync<TEntity, TDto>(
        IQueryable<TEntity> query,
        PaginationParameters pagination,
        Func<List<TEntity>, List<TDto>> mapper,
        CancellationToken cancellationToken = default);
}

public class QueryService : IQueryService
{
    /// <summary>
    /// Applies ordering to a queryable if ordering parameters are specified
    /// </summary>
    public IQueryable<T> ApplyOrdering<T>(IQueryable<T> query, OrderingParameters ordering)
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
    public IQueryable<T> ApplyPagination<T>(IQueryable<T> query, PaginationParameters pagination)
    {
        return query
            .Skip(pagination.Skip)
            .Take(pagination.PageSize);
    }

    /// <summary>
    /// Executes a paginated query and returns a PaginatedList
    /// </summary>
    public async Task<PaginatedList<TDto>> ToPaginatedListAsync<TEntity, TDto>(
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
    /// Applies full-text search filter if specified
    /// </summary>
    public IQueryable<T> ApplyTextSearch<T>(
        IQueryable<T> query,
        FilteringParameters filtering,
        Func<string, IQueryable<T>, IQueryable<T>> searchPredicate)
    {
        if (filtering.HasFilter())
        {
            var searchTerm = filtering.GetLowerSearchTerm();
            query = searchPredicate(searchTerm, query);
        }
        
        return query;
    }
    
    /// <summary>
    /// Applies a specification filter if search term is specified
    /// </summary>
    public IQueryable<TEntity> ApplySpecification<TEntity>(
        IQueryable<TEntity> query,
        FilteringParameters filtering,
        Func<string, IBasicSpecification<TEntity>> specificationFactory)
    {
        if (filtering.HasFilter())
        {
            var searchTerm = filtering.GetLowerSearchTerm();
            var specification = specificationFactory(searchTerm);
            query = query.Where(specification.ToExpression());
        }
        
        return query;
    }

    /// <summary>
    /// Prepares a query by applying search, includes, and ordering (but not pagination)
    /// </summary>
    public IQueryable<TEntity> PrepareQuery<TEntity>(
        IQueryable<TEntity> query,
        FilteringParameters filtering,
        OrderingParameters ordering,
        QueryOptions<TEntity>? options = null)
    {
        if (options != null)
        {
            // Apply search predicate if provided
            if (options.SearchPredicate != null)
            {
                query = ApplyTextSearch(query, filtering, options.SearchPredicate);
            }
            // Otherwise apply specification if provided
            else if (options.SearchSpecificationFactory != null)
            {
                query = ApplySpecification(query, filtering, options.SearchSpecificationFactory);
            }
            
            // Apply includes if provided
            if (options.IncludeFunc != null)
            {
                query = options.IncludeFunc(query);
            }
        }
        
        // Apply ordering
        query = ApplyOrdering(query, ordering);
        
        return query;
    }
    
    /// <summary>
    /// Executes a prepared query and returns paginated results
    /// </summary>
    public async Task<PaginatedList<TDto>> ExecutePaginatedQueryAsync<TEntity, TDto>(
        IQueryable<TEntity> query,
        PaginationParameters pagination,
        Func<List<TEntity>, List<TDto>> mapper,
        CancellationToken cancellationToken = default)
    {
        return await ToPaginatedListAsync(query, pagination, mapper, cancellationToken);
    }
}
