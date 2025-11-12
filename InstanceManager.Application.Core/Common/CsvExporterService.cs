using System.Globalization;
using CsvHelper;
using InstanceManager.Application.Contracts.Modules.Translations;

namespace InstanceManager.Application.Core.Common
{
    public class CsvExporterService : ITranslationExporter
    {
        public string Format => "csv";

        public async Task<Stream> ExportAsync(IEnumerable<TranslationDto> translations, CancellationToken cancellationToken)
        {
            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(translations, cancellationToken);
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
