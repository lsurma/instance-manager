using InstanceManager.Application.Contracts.Modules.Translations;

namespace InstanceManager.Application.Core.Modules.Translations;

public static class TranslationMappingExtensions
{
    public static TranslationDto ToDto(this Translation translation)
    {
        return new TranslationDto
        {
            Id = translation.Id,
            InternalGroupName = translation.InternalGroupName,
            ResourceName = translation.ResourceName,
            TranslationName = translation.TranslationName,
            CultureName = translation.CultureName,
            Content = translation.Content,
            DataSetId = translation.DataSetId,
            CreatedAt = translation.CreatedAt,
            UpdatedAt = translation.UpdatedAt,
            CreatedBy = translation.CreatedBy
        };
    }

    public static List<TranslationDto> ToDto(this List<Translation> translations)
    {
        return translations.Select(t => t.ToDto()).ToList();
    }
}
