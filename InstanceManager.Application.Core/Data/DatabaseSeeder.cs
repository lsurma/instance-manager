using InstanceManager.Application.Core.Modules.ProjectInstance;

namespace InstanceManager.Application.Core.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(InstanceManagerDbContext context)
    {
        // Seed ProjectInstances
        if (!context.ProjectInstances.Any())
        {
            var projects = new List<ProjectInstance>();

            // Create root projects
            var project1 = new ProjectInstance
            {
                Id = Guid.NewGuid(),
                Name = "E-Commerce Platform",
                Description = "Main e-commerce platform for online retail",
                MainHost = "ecommerce.production.example.com",
                Notes = "Production environment. Critical system with 99.9% SLA requirement.",
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-6),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-5),
                CreatedBy = "john.doe@example.com"
            };
            projects.Add(project1);

            var project2 = new ProjectInstance
            {
                Id = Guid.NewGuid(),
                Name = "Mobile Application Suite",
                Description = "Cross-platform mobile application development",
                MainHost = "mobile-api.example.com",
                Notes = "Backend API for mobile apps. Uses GraphQL.",
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-8),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-2),
                CreatedBy = "jane.smith@example.com"
            };
            projects.Add(project2);

            var project3 = new ProjectInstance
            {
                Id = Guid.NewGuid(),
                Name = "Data Analytics Dashboard",
                Description = "Real-time analytics and reporting system",
                MainHost = "analytics.internal.example.com",
                Notes = "Internal use only. Refreshes data every 5 minutes.",
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-4),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                CreatedBy = "bob.wilson@example.com"
            };
            projects.Add(project3);

            // Create child projects for E-Commerce Platform
            var childProject1 = new ProjectInstance
            {
                Id = Guid.NewGuid(),
                Name = "Payment Gateway Integration",
                Description = "Integration with multiple payment providers",
                MainHost = "payments.ecommerce.example.com",
                Notes = "PCI-DSS compliant. Supports Stripe, PayPal, and Square.",
                ParentProjectId = project1.Id,
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-5),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-3),
                CreatedBy = "john.doe@example.com"
            };
            projects.Add(childProject1);

            var childProject2 = new ProjectInstance
            {
                Id = Guid.NewGuid(),
                Name = "Product Catalog Service",
                Description = "Microservice for managing product information",
                MainHost = "catalog.ecommerce.example.com",
                Notes = "Elasticsearch-backed search. 10M+ products indexed.",
                ParentProjectId = project1.Id,
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-5),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-7),
                CreatedBy = "alice.johnson@example.com"
            };
            projects.Add(childProject2);

            // Create child projects for Mobile Application Suite
            var childProject3 = new ProjectInstance
            {
                Id = Guid.NewGuid(),
                Name = "iOS App",
                Description = "Native iOS application",
                MainHost = "mobile-api.example.com",
                Notes = "Swift 5.9, iOS 15+. Deployed via TestFlight for beta testing.",
                ParentProjectId = project2.Id,
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-7),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-4),
                CreatedBy = "jane.smith@example.com"
            };
            projects.Add(childProject3);

            var childProject4 = new ProjectInstance
            {
                Id = Guid.NewGuid(),
                Name = "Android App",
                Description = "Native Android application",
                MainHost = "mobile-api.example.com",
                Notes = "Kotlin, Android 10+. Available on Google Play Store.",
                ParentProjectId = project2.Id,
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-7),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-6),
                CreatedBy = "jane.smith@example.com"
            };
            projects.Add(childProject4);

            // Create a nested child project
            var nestedChildProject = new ProjectInstance
            {
                Id = Guid.NewGuid(),
                Name = "Payment Gateway - Stripe Module",
                Description = "Stripe payment integration module",
                MainHost = "payments.ecommerce.example.com/stripe",
                Notes = "Handles webhooks, subscriptions, and one-time payments.",
                ParentProjectId = childProject1.Id,
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-4),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                CreatedBy = "mike.brown@example.com"
            };
            projects.Add(nestedChildProject);

            // Add standalone project
            var project4 = new ProjectInstance
            {
                Id = Guid.NewGuid(),
                Name = "Internal Tools",
                Description = "Collection of internal development and ops tools",
                MainHost = "tools.internal.example.com",
                Notes = "VPN required for access. Includes deployment scripts and monitoring tools.",
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-3),
                UpdatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "admin@example.com"
            };
            projects.Add(project4);

            context.ProjectInstances.AddRange(projects);
        }

        await context.SaveChangesAsync();
    }
}