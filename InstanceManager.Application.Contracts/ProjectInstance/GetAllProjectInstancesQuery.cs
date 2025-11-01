using MediatR;

namespace InstanceManager.Application.Contracts.ProjectInstance;

public class GetAllProjectInstancesQuery : IRequest<List<ProjectInstanceDto>>
{
    public string OrderBy { get; set; }
    public string OrderDirection { get; set; }
}
