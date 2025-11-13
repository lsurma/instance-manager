using MediatR;

namespace InstanceManager.Application.Contracts.Modules.Mjml;

public class RenderTemplateCommand : IRequest<RenderedTemplateDto>
{
    public string Html { get; set; } = "";
    public string Variables { get; set; } = "";
}
