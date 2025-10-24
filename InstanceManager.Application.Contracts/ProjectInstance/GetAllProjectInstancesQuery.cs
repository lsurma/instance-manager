using MediatR;

namespace InstanceManager.Application.Contracts.ProjectInstance;

public class GetAllProjectInstancesQuery : IRequest<List<ProjectInstanceDto>>
{
}
