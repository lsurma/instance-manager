using System.Linq.Expressions;
using InstanceManager.Application.Core.Common;

namespace InstanceManager.Application.Core.Modules.Translations.Specifications;

public class TranslationSearchSpecification : SearchSpecification<Translation>
{
    public TranslationSearchSpecification(string searchTerm) : base(searchTerm)
    {
    }

    public override Expression<Func<Translation, bool>> ToExpression()
    {
        return t => 
            t.InternalGroupName.ToLower().Contains(SearchTerm) ||
            t.ResourceName.ToLower().Contains(SearchTerm) ||
            t.TranslationName.ToLower().Contains(SearchTerm) ||
            t.Content.ToLower().Contains(SearchTerm);
    }
}
