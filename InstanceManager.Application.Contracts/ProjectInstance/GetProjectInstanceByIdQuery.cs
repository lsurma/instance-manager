using MediatR;

namespace InstanceManager.Application.Contracts.ProjectInstance;

public class GetProjectInstanceByIdQuery : IRequest<ProjectInstanceDto?>
{
    public GetProjectInstanceByIdQuery(Guid id)
    {
        Id = id;
    }
    
    public Guid Id { get; }
}
