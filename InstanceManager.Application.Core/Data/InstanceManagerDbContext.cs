using InstanceManager.Application.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace InstanceManager.Application.Core.Data;

public class InstanceManagerDbContext : DbContext
{
    public InstanceManagerDbContext(DbContextOptions<InstanceManagerDbContext> options) : base(options)
    {
    }

    public DbSet<Modules.ProjectInstance.ProjectInstance> ProjectInstances { get; set; }

    public DbSet<Modules.DataSet.DataSet> DataSets { get; set; }

    public DbSet<Modules.Translations.Translation> Translations { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        SetAuditFields();
        return base.SaveChanges();
    }

    private void SetAuditFields()
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                entry.Entity.CreatedBy = "system"; // TODO: Replace with actual user context
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InstanceManagerDbContext).Assembly);
    }
}
