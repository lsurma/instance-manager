using System.Linq.Expressions;
using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Core.Common;

namespace InstanceManager.Application.Core.Modules.ProjectInstance.Filters;

public class ProjectInstanceSearchFilterHandler : IFilterHandler<ProjectInstance, SearchFilter>
{
    public Task<Expression<Func<ProjectInstance, bool>>> GetFilterExpressionAsync(SearchFilter filter, CancellationToken cancellationToken = default)
    {
        var searchTerm = filter.SearchTerm!.ToLower(); // We know it has value because IsActive() was checked
        Expression<Func<ProjectInstance, bool>> expression = p =>
            p.Name.ToLower().Contains(searchTerm) ||
            (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
            (p.MainHost != null && p.MainHost.ToLower().Contains(searchTerm)) ||
            (p.Notes != null && p.Notes.ToLower().Contains(searchTerm));
        return Task.FromResult(expression);
    }
}
