namespace InstanceManager.Application.Contracts.Modules.Translations;

/// <summary>
/// Lightweight projection of translation data with only essential fields
/// This serves as an example of how to create custom projections
/// </summary>
public record SimpleTranslationDto : ITranslationDto
{
    public Guid Id { get; set; }

    public string TranslationName { get; set; } = string.Empty;

    public string CultureName { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}
