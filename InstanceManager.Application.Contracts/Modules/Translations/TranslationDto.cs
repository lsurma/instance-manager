namespace InstanceManager.Application.Contracts.Modules.Translations;

public record TranslationDto : ITranslationDto
{
    public Guid Id { get; set; }

    public string InternalGroupName { get; set; } = string.Empty;

    public string ResourceName { get; set; } = string.Empty;

    public string TranslationName { get; set; } = string.Empty;

    public string CultureName { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public Guid? DataSetId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}
