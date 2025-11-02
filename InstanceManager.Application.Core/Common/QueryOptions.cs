namespace InstanceManager.Application.Core.Common;

/// <summary>
/// Options for configuring query preparation
/// </summary>
public class QueryOptions<TEntity>
{
    /// <summary>
    /// Function to apply EF Core Include() chains for eager loading
    /// </summary>
    public Func<IQueryable<TEntity>, IQueryable<TEntity>>? IncludeFunc { get; set; }
}
