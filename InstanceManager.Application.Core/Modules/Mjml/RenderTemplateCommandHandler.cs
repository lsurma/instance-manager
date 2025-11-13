using System.Text.Json;
using InstanceManager.Application.Contracts.Modules.Mjml;
using MediatR;
using Scriban;

namespace InstanceManager.Application.Core.Modules.Mjml;

public class RenderTemplateCommandHandler : IRequestHandler<RenderTemplateCommand, RenderedTemplateDto>
{
    public async Task<RenderedTemplateDto> Handle(RenderTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = Template.Parse(request.Html);
        var result = await template.RenderAsync(JsonSerializer.Deserialize<object>(request.Variables));

        return new RenderedTemplateDto { Html = result };
    }
}
