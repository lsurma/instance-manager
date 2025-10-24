using System.Reflection;
using InstanceManager.Application.Contracts;
using InstanceManager.Application.Core.Extensions;
using InstanceManager.Host.AzFuncAPI.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add database
var connectionString = builder.Configuration.GetConnectionString("InstanceManagerDb")
    ?? "Data Source=db/instanceManager.db";
builder.Services.AddInstanceManagerCore(connectionString);

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Find all IRequest from contracts assembly
builder.Services.AddSingleton<RequestRegistry>();


var app = builder.Build();

// Initialize database
await app.Services.InitializeDatabaseAsync();

app.Run();