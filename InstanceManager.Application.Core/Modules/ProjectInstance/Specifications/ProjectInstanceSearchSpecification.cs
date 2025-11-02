using System.Linq.Expressions;
using InstanceManager.Application.Core.Common;

namespace InstanceManager.Application.Core.Modules.ProjectInstance.Specifications;

public class ProjectInstanceSearchSpecification : SearchSpecification<ProjectInstance>
{
    public ProjectInstanceSearchSpecification(string searchTerm) : base(searchTerm)
    {
    }

    public override Expression<Func<ProjectInstance, bool>> ToExpression()
    {
        return p => 
            p.Name.ToLower().Contains(SearchTerm) ||
            (p.Description != null && p.Description.ToLower().Contains(SearchTerm)) ||
            (p.MainHost != null && p.MainHost.ToLower().Contains(SearchTerm)) ||
            (p.Notes != null && p.Notes.ToLower().Contains(SearchTerm));
    }
}
