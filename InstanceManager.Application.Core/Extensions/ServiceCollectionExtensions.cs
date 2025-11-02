using System.Reflection;
using InstanceManager.Application.Core.Common;
using InstanceManager.Application.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InstanceManager.Application.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInstanceManagerCore(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<InstanceManagerDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        
        services.AddScoped<IQueryService, QueryService>();
        services.AddSingleton<IFilterHandlerRegistry, FilterHandlerRegistry>();
        
        // Register all filter handlers
        RegisterFilterHandlers(services);

        return services;
    }
    
    private static void RegisterFilterHandlers(IServiceCollection services)
    {
        var assembly = typeof(ServiceCollectionExtensions).Assembly;
        var filterHandlerType = typeof(IFilterHandler<,>);
        
        // Find all types that implement IFilterHandler<TEntity, TFilter>
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && 
                i.GetGenericTypeDefinition() == filterHandlerType))
            .ToList();
        
        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(handlerType);
        }
    }


    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InstanceManagerDbContext>();

        await context.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(context);
    }
}