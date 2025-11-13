using System.Text.Json;
using InstanceManager.Application.Contracts.Modules.Mjml;
using InstanceManager.Host.AzFuncAPI.Controllers.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace InstanceManager.Host.AzFuncAPI.Controllers;

public class MjmlController
{
    private readonly IMediator _mediator;

    public MjmlController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Function("Mjml/Render")]
    public async Task<IActionResult> Render(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mjml/render")]
        HttpRequest req)
    {
        var mjmlRequest = await JsonSerializer.DeserializeAsync<RenderMjmlRequest>(req.Body);

        if (mjmlRequest?.Html is null || mjmlRequest.Variables is null)
        {
            return new BadRequestObjectResult("Html and Variables are required");
        }

        var command = new RenderTemplateCommand
        {
            Html = mjmlRequest.Html,
            Variables = mjmlRequest.Variables
        };

        var result = await _mediator.Send(command);

        return new OkObjectResult(result);
    }
}
