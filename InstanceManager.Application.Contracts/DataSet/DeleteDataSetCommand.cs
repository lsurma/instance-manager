using MediatR;

namespace InstanceManager.Application.Contracts.DataSet;

public class DeleteDataSetCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
