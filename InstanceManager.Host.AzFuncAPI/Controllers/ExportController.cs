using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Contracts.Modules.Translations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace InstanceManager.Host.AzFuncAPI.Controllers
{
    [Authorize]
    public class ExportController
    {
        private readonly ILogger<ExportController> _logger;
        private readonly IMediator _mediator;

        public ExportController(ILogger<ExportController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [Function("ExportTranslations")]
        public async Task<IActionResult> ExportTranslations(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "export/translations")] HttpRequest req)
        {
            _logger.LogInformation("Processing export translations request.");

            try
            {
                string format = req.Query.ContainsKey("format") ? req.Query["format"].ToString().ToLowerInvariant() : "csv";

                var query = new ExportTranslationsQuery
                {
                    OrderBy = req.Query["orderBy"],
                    OrderDirection = req.Query["orderDirection"],
                    Format = format
                };

                if (req.Query.ContainsKey("filtering"))
                {
                    var filtersJson = req.Query["filtering"];
                    query.Filtering = JsonSerializer.Deserialize<FilteringParameters>(filtersJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new QueryFilterJsonConverter() }
                    });
                }

                var resultStream = await _mediator.Send(query);

                string contentType;
                string fileExtension;

                switch (format)
                {
                    case "xlsx":
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        fileExtension = "xlsx";
                        break;
                    default:
                        contentType = "text/csv";
                        fileExtension = "csv";
                        break;
                }

                return new FileStreamResult(resultStream, contentType)
                {
                    FileDownloadName = $"translations_{DateTime.UtcNow:yyyyMMddHHmmss}.{fileExtension}"
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize filters for export translations request.");
                return new BadRequestObjectResult(new { error = "Invalid JSON in filters parameter.", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing export translations request.");
                return new ObjectResult(new { error = ex.Message }) { StatusCode = 500 };
            }
        }
    }
}
