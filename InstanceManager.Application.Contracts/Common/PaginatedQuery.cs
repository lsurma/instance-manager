using MediatR;

namespace InstanceManager.Application.Contracts.Common;

public abstract class PaginatedQuery<TResponse> : IRequest<PaginatedList<TResponse>>
{
    public PaginationParameters Pagination { get; set; } = new();
    
    public OrderingParameters Ordering { get; set; } = new();
    
    public FilteringParameters Filtering { get; set; } = new();

    /// <summary>
    /// Creates a query that returns all items without pagination
    /// </summary>
    public static TPaginatedQuery AllItems<TPaginatedQuery>(string? orderBy = null, string? orderDirection = null) 
        where TPaginatedQuery : PaginatedQuery<TResponse>, new()
    {
        return new TPaginatedQuery
        {
            Pagination = PaginationParameters.AllItems(),
            Ordering = new OrderingParameters
            {
                OrderBy = orderBy,
                OrderDirection = orderDirection
            }
        };
    }
}
