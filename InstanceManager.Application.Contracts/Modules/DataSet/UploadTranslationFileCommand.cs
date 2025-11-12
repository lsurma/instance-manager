using MediatR;

namespace InstanceManager.Application.Contracts.Modules.DataSets
{
    public class UploadTranslationFileCommand : IRequest
    {
        public Guid DataSetId { get; set; }
        public string? FileName { get; set; }
        public Stream? Content { get; set; }
    }
}
