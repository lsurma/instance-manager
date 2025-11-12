using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Contracts.Modules.Translations;
using InstanceManager.Application.Core.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Modules.Translations.Handlers;

public class ExportTranslationsQueryHandler : IRequestHandler<ExportTranslationsQuery, Stream>
{
    private readonly TranslationsQueryService _queryService;
    private readonly TranslationExporterFactory _exporterFactory;

    public ExportTranslationsQueryHandler(TranslationsQueryService queryService, TranslationExporterFactory exporterFactory)
    {
        _queryService = queryService;
        _exporterFactory = exporterFactory;
    }

    public async Task<Stream> Handle(ExportTranslationsQuery request, CancellationToken cancellationToken)
    {
        var queryOptions = new TranslationsQueryService.Options
        {
            Ordering = new OrderingParameters
            {
                OrderBy = request.OrderBy,
                OrderDirection = request.OrderDirection
            },
            Filtering = new FilteringParameters
            {
                QueryFilters = request.Filtering?.QueryFilters
            }
        };

        var query = await _queryService.PrepareQueryAsync(options: queryOptions, cancellationToken: cancellationToken);
        var translations = await query.ToListAsync(cancellationToken);
        var translationDtos = translations.ToDto();

        var exporter = _exporterFactory.GetExporter(request.Format);
        return await exporter.ExportAsync(translationDtos, cancellationToken);
    }
}
