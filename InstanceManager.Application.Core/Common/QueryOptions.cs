using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Core.Abstractions;

namespace InstanceManager.Application.Core.Common;

/// <summary>
/// Options for configuring query preparation
/// </summary>
public class QueryOptions<TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
    where TPrimaryKey : notnull
{
    /// <summary>
    /// Function to apply EF Core Include() chains for eager loading
    /// </summary>
    public Func<IQueryable<TEntity>, IQueryable<TEntity>>? IncludeFunc { get; set; }

    /// <summary>
    /// Filtering parameters for the query
    /// </summary>
    public FilteringParameters? Filtering { get; set; }

    /// <summary>
    /// Ordering parameters for the query
    /// </summary>
    public OrderingParameters? Ordering { get; set; }
}

/// <summary>
/// Options for configuring query preparation for entities with Guid primary key (backward compatibility)
/// </summary>
public class QueryOptions<TEntity> : QueryOptions<TEntity, Guid>
    where TEntity : class, IEntity<Guid>
{
}
