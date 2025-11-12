using InstanceManager.Application.Core.Common;
using InstanceManager.Application.Core.Extensions;
using InstanceManager.Authentication.Core;
using InstanceManager.Host.AzFuncAPI.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
var builder = FunctionsApplication.CreateBuilder(args);

// Configure Functions Web Application with middleware pipeline
var functionsApp = builder.ConfigureFunctionsWebApplication()
    .UseInstanceManagerAuthentication();

// Add HTTP context accessor for user identity tracking
builder.Services.AddHttpContextAccessor();

// Add database
var connectionString = builder.Configuration.GetConnectionString("InstanceManagerDb")
    ?? "Data Source=db/instanceManager.db";
builder.Services.AddInstanceManagerCore(connectionString, authOptions =>
{
    // Configure root users from configuration or environment
    var rootUsers = builder.Configuration.GetSection("Authorization:RootUsers").Get<string[]>();
    if (rootUsers != null)
    {
        foreach (var userId in rootUsers)
        {
            authOptions.AddRootUser(userId);
        }
    }

    // For development, you can also hard-code root users here:
    // authOptions.AddRootUser("admin@example.com");
    // authOptions.AddRootUser("api-key-admin");
});

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Add authentication services
builder.Services.AddInstanceManagerAuthentication(builder.Configuration);

// Find all IRequest from contracts assembly
builder.Services.AddSingleton<RequestRegistry>();
builder.Services.AddSingleton<ITranslationExporter, CsvExporterService>();
builder.Services.AddSingleton<ITranslationExporter, ExcelExporterService>();
builder.Services.AddSingleton<TranslationExporterFactory>();


var app = builder.Build();

// Initialize database
await app.Services.InitializeDatabaseAsync();

app.Run();