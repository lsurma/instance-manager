using InstanceManager.Host.AzFuncAPI.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InstanceManager.Host.AzFuncAPI.Controllers;

[Authorize]
public class QueryController
{
    private readonly ILogger<QueryController> _logger;
    private readonly IMediator _mediator;
    private readonly RequestRegistry _requestRegistry;

    public QueryController(ILogger<QueryController> logger, IMediator mediator, RequestRegistry requestRegistry)
    {
        _logger = logger;
        _mediator = mediator;
        _requestRegistry = requestRegistry;
    }

    /// <summary>
    /// Main query endpoint that routes MediatR requests.
    /// Authentication is handled by the authorization middleware.
    /// Supports both JWT Bearer tokens (Authorization: Bearer {token}) and API Keys (X-API-Key: {key}).
    /// </summary>
    [Function("Query")]
    public async Task<IActionResult> Query(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "query/{requestName}")] HttpRequest req,
        string requestName
    )
    {
        var bodyJson = req.Query["body"].ToString();

        if (string.IsNullOrWhiteSpace(requestName))
        {
            return new BadRequestObjectResult(new { error = "Request name is required." });
        }

        _logger.LogInformation("Processing request: {RequestName}", requestName);

        try
        {
            var requestType = _requestRegistry.GetRequestType(requestName);
            if (requestType == null)
            {
                return new NotFoundObjectResult(new
                {
                    error = $"Request '{requestName}' not found.",
                    availableRequests = _requestRegistry.GetAllRequestNames()
                });
            }

            // Deserialize body if provided, otherwise create empty instance
            object? request;
            if (!string.IsNullOrWhiteSpace(bodyJson))
            {
                request = JsonSerializer.Deserialize(bodyJson, requestType, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                request = Activator.CreateInstance(requestType);
            }

            if (request == null)
            {
                return new BadRequestObjectResult(new { error = "Failed to create request instance." });
            }

            // Send through MediatR
            var result = await _mediator.Send(request);

            return new JsonResult(result, new JsonSerializerOptions()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize request body for: {RequestName}", requestName);
            return new BadRequestObjectResult(new { error = "Invalid JSON in body parameter.", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request: {RequestName}", requestName);
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

}