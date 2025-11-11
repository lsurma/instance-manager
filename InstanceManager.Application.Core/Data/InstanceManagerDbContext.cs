using InstanceManager.Authentication.Core;
using InstanceManager.Application.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace InstanceManager.Application.Core.Data;

public class InstanceManagerDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public InstanceManagerDbContext(
        DbContextOptions<InstanceManagerDbContext> options,
        ICurrentUserService? currentUserService = null) : base(options)
    {
        _currentUserService = currentUserService;
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
        var currentUserId = _currentUserService?.GetUserId() ?? "system";

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                entry.Entity.CreatedBy = currentUserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                entry.Entity.UpdatedBy = currentUserId;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InstanceManagerDbContext).Assembly);
    }
}
