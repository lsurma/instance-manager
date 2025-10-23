using InstanceManager.Application.Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace InstanceManager.Host.AzFuncAPI.Controllers;

public class QueryController
{
    private readonly ILogger<QueryController> _logger;
    private readonly InstanceManagerDbContext _context;

    public QueryController(ILogger<QueryController> logger, InstanceManagerDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Function("Query")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        string[] weatherForecasts = [];
        
        return new JsonResult(weatherForecasts);
    }

}