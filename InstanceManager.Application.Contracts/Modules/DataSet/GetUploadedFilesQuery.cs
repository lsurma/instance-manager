using MediatR;

namespace InstanceManager.Application.Contracts.Modules.DataSets
{
    public class GetUploadedFilesQuery : IRequest<List<UploadedFileDto>>
    {
        public Guid DataSetId { get; set; }
    }
}
