using MediatR;

namespace InstanceManager.Application.Contracts.DataSet;

public class GetDataSetByIdQuery : IRequest<DataSetDto?>
{
    public Guid Id { get; set; }
}
