using InstanceManager.Application.Contracts.Modules.DataSets;
using MediatR;

namespace InstanceManager.Application.Core.Modules.DataSet
{
    public class UploadTranslationFileCommandHandler : IRequestHandler<UploadTranslationFileCommand>
    {
        public async Task Handle(UploadTranslationFileCommand request, CancellationToken cancellationToken)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"{request.DataSetId}_{request.FileName}");

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Content.CopyToAsync(stream, cancellationToken);
            }
        }
    }
}
