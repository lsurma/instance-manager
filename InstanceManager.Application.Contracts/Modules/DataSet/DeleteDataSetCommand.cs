using MediatR;

namespace InstanceManager.Application.Contracts.Modules.DataSet;

public class DeleteDataSetCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
