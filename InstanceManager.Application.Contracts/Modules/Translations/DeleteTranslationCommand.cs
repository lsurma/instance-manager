using MediatR;

namespace InstanceManager.Application.Contracts.Modules.Translations;

public class DeleteTranslationCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
