namespace InstanceManager.Application.Core.Common;

public class QueryOptions<TEntity>
{
    public Func<string, IQueryable<TEntity>, IQueryable<TEntity>>? SearchPredicate { get; set; }
    
    public Func<string, IBasicSpecification<TEntity>>? SearchSpecificationFactory { get; set; }
    
    public Func<IQueryable<TEntity>, IQueryable<TEntity>>? IncludeFunc { get; set; }
}
