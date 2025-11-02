using InstanceManager.Application.Contracts.Common;

namespace InstanceManager.Application.Core.Common;

/// <summary>
/// Registry for filter handlers, automatically populated from DI
/// </summary>
public interface IFilterHandlerRegistry
{
    /// <summary>
    /// Gets all registered filter handlers for a specific entity type
    /// </summary>
    Dictionary<Type, object> GetHandlersForEntity<TEntity>();
}
