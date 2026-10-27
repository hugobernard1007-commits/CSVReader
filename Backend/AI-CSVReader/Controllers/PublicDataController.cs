using CsvApi.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CsvApi.Backend.Controllers;

[ApiController]
[Route("api/public")]
public class PublicDataController : ControllerBase
{
    private readonly IDynamicEndpointService _endpointService;
    private readonly ICsvDatasetService _datasetService;

    public PublicDataController(
        IDynamicEndpointService endpointService,
        ICsvDatasetService datasetService)
    {
        _endpointService = endpointService;
        _datasetService = datasetService;
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<object>> GetPublicData(
        string slug,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        [FromQuery] string? search = null)
    {
        var endpoint = await _endpointService.GetBySlugAsync(slug);

        if (endpoint == null)
        {
            return NotFound(new
            {
                message = "Endpoint public introuvable."
            });
        }

        var dataset = await _datasetService.GetByIdAsync(endpoint.DatasetId);

        if (dataset == null)
        {
            return NotFound(new
            {
                message = "Le dataset associé à cet endpoint est introuvable."
            });
        }

        var query = dataset.Rows.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(row =>
                row.Values.Any(value =>
                    value.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        var projectedRows = query.Select(row =>
        {
            var output = new Dictionary<string, string>();

            foreach (var column in endpoint.Columns)
            {
                output[column] = row.TryGetValue(column, out var value)
                    ? value
                    : string.Empty;
            }

            return output;
        });

        var total = projectedRows.Count();

        var rows = projectedRows
            .Skip(Math.Max(skip, 0))
            .Take(Math.Clamp(take, 1, 1000))
            .ToList();

        return Ok(new
        {
            Endpoint = endpoint.Name,
            endpoint.Slug,
            endpoint.Url,
            endpoint.Description,
            Columns = endpoint.Columns,
            Total = total,
            Skip = skip,
            Take = take,
            Rows = rows
        });
    }
}