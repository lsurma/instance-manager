using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstanceManager.Application.Contracts.Modules.DataSets;
using MediatR;

namespace InstanceManager.Application.Core.Modules.DataSet
{
    public class GetUploadedFilesQueryHandler : IRequestHandler<GetUploadedFilesQuery, List<UploadedFileDto>>
    {
        public Task<List<UploadedFileDto>> Handle(GetUploadedFilesQuery request, CancellationToken cancellationToken)
        {
            var searchPattern = $"{request.DataSetId}_*";
            var files = Directory.GetFiles(Path.GetTempPath(), searchPattern)
                .Select(Path.GetFileName)
                .Select(fileName => new UploadedFileDto { FileName = fileName.Replace($"{request.DataSetId}_", "") })
                .ToList();

            return Task.FromResult(files);
        }
    }
}
