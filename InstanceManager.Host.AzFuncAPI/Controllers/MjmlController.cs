using System.Text.Json;
using InstanceManager.Host.AzFuncAPI.Controllers.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Scriban;

namespace InstanceManager.Host.AzFuncAPI.Controllers;

public class MjmlController
{
    [Function("Mjml/Render")]
    public static async Task<IActionResult> Render(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mjml/render")]
        HttpRequest req)
    {
        var mjmlRequest = await JsonSerializer.DeserializeAsync<RenderMjmlRequest>(req.Body);

        if (mjmlRequest?.Html is null || mjmlRequest.Variables is null)
        {
            return new BadRequestObjectResult("Html and Variables are required");
        }

        var template = Template.Parse(mjmlRequest.Html);
        var result = await template.RenderAsync(JsonSerializer.Deserialize<object>(mjmlRequest.Variables));

        return new OkObjectResult(new { html = result });
    }
}
