using MediatR;

namespace InstanceManager.Application.Contracts.ProjectInstance;

public class DeleteProjectInstanceCommand : IRequest<bool>
{
    public DeleteProjectInstanceCommand(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}
