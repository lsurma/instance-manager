namespace InstanceManager.Application.Core.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(InstanceManagerDbContext context)
    {
        await context.SaveChangesAsync();
    }
}