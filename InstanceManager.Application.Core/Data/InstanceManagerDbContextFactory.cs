using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace InstanceManager.Application.Core.Data;

public class InstanceManagerDbContextFactory : IDesignTimeDbContextFactory<InstanceManagerDbContext>
{
    public InstanceManagerDbContext CreateDbContext(string[] args)
    {
        // Build configuration to read from local.settings.json or appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("InstanceManagerDb")
            ?? "Data Source=db/instanceManager.db";

        var optionsBuilder = new DbContextOptionsBuilder<InstanceManagerDbContext>();
        optionsBuilder.UseSqlite(connectionString);

        return new InstanceManagerDbContext(optionsBuilder.Options);
    }
}
