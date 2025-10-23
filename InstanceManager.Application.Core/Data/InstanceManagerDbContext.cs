using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Data;

public class InstanceManagerDbContext : DbContext
{
    public InstanceManagerDbContext(DbContextOptions<InstanceManagerDbContext> options) : base(options)
    {
    }
}