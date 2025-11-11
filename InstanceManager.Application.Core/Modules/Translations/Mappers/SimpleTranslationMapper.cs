using System.Linq.Expressions;
using InstanceManager.Application.Contracts.Modules.Translations;

namespace InstanceManager.Application.Core.Modules.Translations.Mappers;

/// <summary>
/// Projection mapper for SimpleTranslationDto
/// This serves as an example of how to implement projection mappers
/// </summary>
public class SimpleTranslationMapper : ITranslationProjectionMapper<SimpleTranslationDto>
{
    public Expression<Func<Translation, SimpleTranslationDto>> GetSelector()
    {
        return t => new SimpleTranslationDto
        {
            Id = t.Id,
            TranslationName = t.TranslationName,
            CultureName = t.CultureName,
            Content = t.Content
        };
    }
}
