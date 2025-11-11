using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InstanceManager.Application.Contracts.Modules.Translations;

namespace InstanceManager.Application.Core.Common
{
    public interface ITranslationExporter
    {
        string Format { get; }
        Task<Stream> ExportAsync(IEnumerable<TranslationDto> translations, CancellationToken cancellationToken);
    }
}
