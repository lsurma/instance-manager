using InstanceManager.Application.Contracts.Common;

namespace InstanceManager.Application.Contracts.Modules.Translations;

public class GetTranslationsQuery : PaginatedQuery<TranslationDto>
{
    
    /// <summary>
    /// Creates a query that returns all items without pagination
    /// </summary>
    public static GetTranslationsQuery AllItems(string? orderBy = null, string? orderDirection = null)
    {
        return AllItems<GetTranslationsQuery>(orderBy, orderDirection);
    }
}
