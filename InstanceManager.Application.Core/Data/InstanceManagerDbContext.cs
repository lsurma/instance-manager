using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Data;

public class InstanceManagerDbContext : DbContext
{
    public InstanceManagerDbContext(DbContextOptions<InstanceManagerDbContext> options) : base(options)
    {
    }

    public DbSet<Modules.ProjectInstance.ProjectInstance> ProjectInstances { get; set; }

    public DbSet<Modules.DataSet.DataSet> DataSets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InstanceManagerDbContext).Assembly);
    }
}