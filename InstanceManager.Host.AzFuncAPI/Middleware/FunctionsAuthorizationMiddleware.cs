using System.Net;
using InstanceManager.Host.AzFuncAPI.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace InstanceManager.Host.AzFuncAPI.Middleware;

/// <summary>
/// Middleware that enforces authentication for Azure Functions based on configuration.
/// This bridges the gap between Azure Functions and ASP.NET Core authorization.
/// </summary>
public class FunctionsAuthorizationMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<FunctionsAuthorizationMiddleware> _logger;
    private readonly AuthenticationSettings _authSettings;

    public FunctionsAuthorizationMiddleware(
        ILogger<FunctionsAuthorizationMiddleware> logger,
        AuthenticationSettings authSettings)
    {
        _logger = logger;
        _authSettings = authSettings;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        // Get the HTTP request data
        var httpRequestData = await context.GetHttpRequestDataAsync();
        if (httpRequestData == null)
        {
            // Not an HTTP trigger, skip authorization
            await next(context);
            return;
        }

        // Skip authentication check if not required or APIM bypass is active
        if (!_authSettings.RequireAuthentication || _authSettings.Apim.TrustApim)
        {
            _logger.LogDebug("Authentication not required or APIM bypass active");
            await next(context);
            return;
        }

        // Get HttpContext from ASP.NET Core integration
        var httpContext = context.GetHttpContext();
        if (httpContext == null)
        {
            _logger.LogWarning("HttpContext is null - unable to perform authorization");

            // Return 401 Unauthorized response
            var response = httpRequestData.CreateResponse(HttpStatusCode.Unauthorized);
            await response.WriteAsJsonAsync(new { error = "Unauthorized - HttpContext not available" });
            context.GetInvocationResult().Value = response;
            return;
        }

        // Check if user is authenticated
        var isAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false;
        var userName = httpContext.User.Identity?.Name ?? "anonymous";

        if (!isAuthenticated)
        {
            _logger.LogWarning("Unauthorized request | User not authenticated");

            // Return 401 Unauthorized response
            var response = httpRequestData.CreateResponse(HttpStatusCode.Unauthorized);
            await response.WriteAsJsonAsync(new
            {
                error = "Unauthorized",
                message = "Authentication is required. Please provide valid credentials.",
                authMethods = new[]
                {
                    _authSettings.ApiKeys.Enabled ? "API Key (X-API-Key header)" : null,
                    _authSettings.EntraId.Enabled ? "JWT Bearer Token (Authorization header)" : null
                }.Where(m => m != null).ToArray()
            });
            context.GetInvocationResult().Value = response;
            return;
        }

        _logger.LogInformation(
            "Request authorized | User: {User} | Authenticated: {IsAuthenticated}",
            userName,
            isAuthenticated);

        await next(context);
    }
}
