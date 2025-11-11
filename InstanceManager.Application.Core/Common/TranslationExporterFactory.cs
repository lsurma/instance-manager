using System;
using System.Collections.Generic;
using System.Linq;

namespace InstanceManager.Application.Core.Common
{
    public class TranslationExporterFactory
    {
        private readonly IEnumerable<ITranslationExporter> _exporters;

        public TranslationExporterFactory(IEnumerable<ITranslationExporter> exporters)
        {
            _exporters = exporters;
        }

        public ITranslationExporter GetExporter(string format)
        {
            var exporter = _exporters.FirstOrDefault(e => e.Format.Equals(format, StringComparison.OrdinalIgnoreCase));
            if (exporter == null)
            {
                throw new NotSupportedException($"Export format '{format}' is not supported.");
            }
            return exporter;
        }
    }
}
