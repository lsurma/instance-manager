using System.Linq.Expressions;

namespace InstanceManager.Application.Core.Common;

public abstract class SearchSpecification<TEntity> : IBasicSpecification<TEntity>
{
    protected string SearchTerm { get; }

    protected SearchSpecification(string searchTerm)
    {
        SearchTerm = searchTerm.ToLower();
    }

    public abstract Expression<Func<TEntity, bool>> ToExpression();
}
