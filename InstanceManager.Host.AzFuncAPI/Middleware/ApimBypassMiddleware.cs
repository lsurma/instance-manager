using System.Security.Claims;
using InstanceManager.Host.AzFuncAPI.Authentication;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace InstanceManager.Host.AzFuncAPI.Middleware;

/// <summary>
/// Middleware that bypasses authentication when requests come from Azure API Management.
/// This is useful for testing APIM integration where APIM handles authentication.
/// </summary>
public class ApimBypassMiddleware : IFunctionsWorkerMiddleware
{
    private const string ApimSecretHeaderName = "X-APIM-Secret";
    private readonly AuthenticationSettings _authSettings;
    private readonly ILogger<ApimBypassMiddleware> _logger;

    public ApimBypassMiddleware(AuthenticationSettings authSettings, ILogger<ApimBypassMiddleware> logger)
    {
        _authSettings = authSettings;
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        // Only process if APIM trust is enabled
        if (!_authSettings.Apim.TrustApim)
        {
            await next(context);
            return;
        }

        // Check if the request has the APIM secret header
        if (_authSettings.Apim.RequireSharedSecret)
        {
            var httpRequestData = await context.GetHttpRequestDataAsync();
            if (httpRequestData == null)
            {
                await next(context);
                return;
            }

            // Check for the shared secret header
            if (!httpRequestData.Headers.TryGetValues(ApimSecretHeaderName, out var secretValues))
            {
                _logger.LogWarning("APIM bypass enabled but X-APIM-Secret header not found");
                await next(context);
                return;
            }

            var providedSecret = secretValues.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(providedSecret) || providedSecret != _authSettings.Apim.SharedSecret)
            {
                _logger.LogWarning("APIM bypass enabled but X-APIM-Secret header value is invalid");
                await next(context);
                return;
            }

            _logger.LogInformation("Request authenticated via APIM shared secret - bypassing normal authentication");
        }
        else
        {
            _logger.LogWarning("APIM bypass enabled WITHOUT shared secret requirement - this should only be used for testing!");
        }

        // Create a synthetic authenticated user for APIM requests
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "APIM"),
            new Claim(ClaimTypes.NameIdentifier, "apim-gateway"),
            new Claim("AuthenticationMethod", "APIM-Bypass")
        };

        var identity = new ClaimsIdentity(claims, "APIM");
        var principal = new ClaimsPrincipal(identity);

        // Store the principal in the function context items
        // Note: This is a simplified approach for Functions
        // The actual authentication in HTTP pipeline will still run, but this shows intent
        context.Items["User"] = principal;

        await next(context);
    }
}
