using MediatR;

namespace InstanceManager.Application.Contracts.Modules.Translations;

public class GetTranslationByIdQuery : IRequest<TranslationDto?>
{
    public Guid Id { get; set; }
}
