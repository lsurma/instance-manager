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

        return services;
    }


    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InstanceManagerDbContext>();

        await context.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(context);
    }
}