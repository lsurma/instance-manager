using ClosedXML.Excel;
using InstanceManager.Application.Contracts.Modules.Translations;

namespace InstanceManager.Application.Core.Common
{
    public class ExcelExporterService : ITranslationExporter
    {
        public string Format => "xlsx";

        public Task<Stream> ExportAsync(IEnumerable<TranslationDto> translations, CancellationToken cancellationToken)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Translations");

            worksheet.Cell(1, 1).InsertTable(translations);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return Task.FromResult<Stream>(stream);
        }
    }
}
