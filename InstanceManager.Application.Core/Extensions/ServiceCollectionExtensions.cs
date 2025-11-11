using System.Reflection;
using InstanceManager.Authentication.Core;
using InstanceManager.Application.Core.Common;
using InstanceManager.Application.Core.Data;
using InstanceManager.Application.Core.Modules.DataSet;
using InstanceManager.Application.Core.Modules.ProjectInstance;
using InstanceManager.Application.Core.Modules.Translations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InstanceManager.Application.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInstanceManagerCore(
        this IServiceCollection services,
        string connectionString,
        Action<AuthorizationOptions>? configureAuthorization = null)
    {
        services.AddDbContext<InstanceManagerDbContext>(options =>
            options.UseSqlite(connectionString));

        // Register MediatR with logging pipeline behavior
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        // Register user context (populated by middleware in Azure Functions)
        services.AddScoped<UserContext>();

        // Register current user service
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register authorization service with options
        var authOptions = new AuthorizationOptions();
        configureAuthorization?.Invoke(authOptions);
        services.AddSingleton(authOptions);
        services.AddScoped<IAuthorizationService, AuthorizationService>();

        // Register entity-specific query services
        services.AddScoped<IQueryService<DataSet, Guid>, DataSetsQueryService>();
        services.AddScoped<IQueryService<ProjectInstance, Guid>, ProjectInstancesQueryService>();
        services.AddScoped<IQueryService<Translation, Guid>, TranslationsQueryService>();

        // Also register specialized query services directly for injection when needed
        services.AddScoped<DataSetsQueryService>();
        services.AddScoped<TranslationsQueryService>();

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