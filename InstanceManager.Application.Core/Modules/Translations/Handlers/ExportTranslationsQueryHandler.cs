using CsvHelper;
using MediatR;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using InstanceManager.Application.Contracts.Modules.Translations;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using InstanceManager.Application.Contracts.Common;

namespace InstanceManager.Application.Core.Modules.Translations.Handlers;

public class ExportTranslationsQueryHandler : IRequestHandler<ExportTranslationsQuery, Stream>
{
    private readonly TranslationsQueryService _queryService;

    public ExportTranslationsQueryHandler(TranslationsQueryService queryService)
    {
        _queryService = queryService;
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

        var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            await csv.WriteRecordsAsync(translationDtos, cancellationToken);
        }

        memoryStream.Position = 0;
        return memoryStream;
    }
}
