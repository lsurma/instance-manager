using System.Net;
using System.Security.Claims;
using InstanceManager.Authentication.Core.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InstanceManager.Authentication.Core.Middleware;

/// <summary>
/// Middleware that enforces authentication for Azure Functions based on configuration.
/// This bridges the gap between Azure Functions and ASP.NET Core authorization.
/// Also populates the UserContext for reliable user identity access.
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

        // Get HttpContext from ASP.NET Core integration
        var httpContext = context.GetHttpContext();

        // Get UserContext from DI to populate for downstream services
        var userContext = context.InstanceServices.GetService<UserContext>();

        // Skip authentication check if not required or APIM bypass is active
        if (!_authSettings.RequireAuthentication || _authSettings.Apim.TrustApim)
        {
            _logger.LogDebug("Authentication not required or APIM bypass active");

            // Still try to populate UserContext if HttpContext is available
            if (httpContext != null && userContext != null)
            {
                userContext.User = httpContext.User ?? new ClaimsPrincipal();
                _logger.LogDebug("Populated UserContext (no auth required mode)");
            }

            await next(context);
            return;
        }

        if (httpContext == null)
        {
            _logger.LogWarning("HttpContext is null - unable to perform authorization");

            // Return 401 Unauthorized response
            var response = httpRequestData.CreateResponse(HttpStatusCode.Unauthorized);
            await response.WriteAsJsonAsync(new { error = "Unauthorized - HttpContext not available" });
            context.GetInvocationResult().Value = response;
            return;
        }

        // IMPORTANT: Manually trigger authentication since ASP.NET Core middleware may not run automatically
        // Try API Key authentication first if enabled
        AuthenticateResult? authenticateResult = null;

        if (_authSettings.ApiKeys.Enabled)
        {
            authenticateResult = await httpContext.AuthenticateAsync(ApiKeyAuthenticationOptions.DefaultScheme);
            _logger.LogDebug("API Key authentication result: Success={Success}", authenticateResult.Succeeded);

            // CRITICAL: Set the HttpContext.User from the authentication result
            if (authenticateResult.Succeeded && authenticateResult.Principal != null)
            {
                httpContext.User = authenticateResult.Principal;
                _logger.LogDebug("Set HttpContext.User from API Key authentication");
            }
        }

        // If API Key failed and JWT is enabled, try JWT
        if ((authenticateResult == null || !authenticateResult.Succeeded) && _authSettings.EntraId.Enabled)
        {
            authenticateResult = await httpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            _logger.LogDebug("JWT authentication result: Success={Success}", authenticateResult?.Succeeded);

            // CRITICAL: Set the HttpContext.User from the authentication result
            if (authenticateResult?.Succeeded == true && authenticateResult.Principal != null)
            {
                httpContext.User = authenticateResult.Principal;
                _logger.LogDebug("Set HttpContext.User from JWT authentication");
            }
        }

        // Populate UserContext with the authenticated user (if available)
        // This is more reliable than IHttpContextAccessor in Azure Functions
        if (userContext != null && httpContext.User != null)
        {
            userContext.User = httpContext.User;
            _logger.LogDebug("Populated UserContext with ClaimsPrincipal");
        }

        // Check if user is authenticated
        var isAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false;
        var userName = httpContext.User.Identity?.Name ?? "anonymous";

        _logger.LogDebug("Final authentication status: IsAuthenticated={IsAuthenticated}, UserName={UserName}",
            isAuthenticated, userName);

        if (!isAuthenticated)
        {
            _logger.LogWarning("Unauthorized request | User not authenticated | Headers: {Headers}",
                string.Join(", ", httpContext.Request.Headers.Select(h => $"{h.Key}={h.Value}")));

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
