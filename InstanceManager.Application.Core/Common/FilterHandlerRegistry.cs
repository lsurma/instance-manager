using InstanceManager.Application.Contracts.Common;

namespace InstanceManager.Application.Core.Common;

public class FilterHandlerRegistry : IFilterHandlerRegistry
{
    private readonly IServiceProvider _serviceProvider;

    public FilterHandlerRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Dictionary<Type, object> GetHandlersForEntity<TEntity>()
    {
        var handlers = new Dictionary<Type, object>();
        
        // Get all registered services from DI
        var filterHandlerType = typeof(IFilterHandler<,>);
        
        // Find all types that implement IFilterHandler<TEntity, TFilter>
        var assembly = typeof(FilterHandlerRegistry).Assembly;
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && 
                i.GetGenericTypeDefinition() == filterHandlerType &&
                i.GetGenericArguments()[0] == typeof(TEntity)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            // Get the filter type (second generic argument)
            var interfaceType = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && 
                           i.GetGenericTypeDefinition() == filterHandlerType &&
                           i.GetGenericArguments()[0] == typeof(TEntity));
            
            var filterType = interfaceType.GetGenericArguments()[1];
            
            // Get or create instance from DI
            var handler = _serviceProvider.GetService(handlerType) ?? Activator.CreateInstance(handlerType);
            
            if (handler != null)
            {
                handlers[filterType] = handler;
            }
        }

        return handlers;
    }
}
