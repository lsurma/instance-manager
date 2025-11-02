using MediatR;

namespace InstanceManager.Application.Contracts.Modules.Translations;

public class SaveTranslationCommand : IRequest<Guid>
{
    public Guid? Id { get; set; }

    public required string InternalGroupName { get; set; }

    public required string ResourceName { get; set; }

    public required string TranslationName { get; set; }

    public required string CultureName { get; set; }

    public required string Content { get; set; }

    public Guid? DataSetId { get; set; }
}
