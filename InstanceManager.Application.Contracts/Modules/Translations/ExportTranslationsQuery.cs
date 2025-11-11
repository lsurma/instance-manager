using System.IO;
using InstanceManager.Application.Contracts.Common;
using MediatR;

namespace InstanceManager.Application.Contracts.Modules.Translations;

public class ExportTranslationsQuery : IRequest<Stream>
{
    public string? OrderBy { get; set; }
    public string? OrderDirection { get; set; }
    public FilteringParameters? Filtering { get; set; }
    public string Format { get; set; } = "csv";
}
