using System.Linq.Expressions;

namespace InstanceManager.Application.Core.Common;

public interface IBasicSpecification<TEntity>
{
    Expression<Func<TEntity, bool>> ToExpression();
}
