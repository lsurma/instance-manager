using System;
using MediatR;

namespace InstanceManager.Application.Contracts.Modules.DataSets
{
    public class ProcessTranslationFileCommand : IRequest
    {
        public Guid DataSetId { get; set; }
        public string? FileName { get; set; }
    }
}
