using CsvApi.Backend.Models;
using CsvApi.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CsvApi.Backend.Controllers;

[ApiController]
[Route("api/datasets")]
public class DatasetsController : ControllerBase
{
    private readonly ICsvDatasetService _datasetService;

    public DatasetsController(ICsvDatasetService datasetService)
    {
        _datasetService = datasetService;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<CsvUploadResponse>> Upload([FromForm] IFormFile file)
    {
        try
        {
            var dataset = await _datasetService.UploadAsync(file);

            var response = new CsvUploadResponse
            {
                DatasetId = dataset.Id,
                Name = dataset.Name,
                OriginalFileName = dataset.OriginalFileName,
                RowCount = dataset.Rows.Count,
                ColumnCount = dataset.Columns.Count,
                Columns = dataset.Columns
            };

            return Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<object>>> GetAll()
    {
        var datasets = await _datasetService.GetAllAsync();

        var response = datasets.Select(dataset => new
        {
            dataset.Id,
            dataset.Name,
            dataset.OriginalFileName,
            dataset.UploadedAt,
            RowCount = dataset.Rows.Count,
            ColumnCount = dataset.Columns.Count,
            dataset.Columns
        });

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CsvDataset>> GetById(Guid id)
    {
        var dataset = await _datasetService.GetByIdAsync(id);

        if (dataset == null)
        {
            return NotFound(new
            {
                message = "Dataset introuvable."
            });
        }

        return Ok(dataset);
    }

    [HttpGet("{id:guid}/rows")]
    public async Task<ActionResult<object>> GetRows(
        Guid id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        [FromQuery] string? search = null)
    {
        var dataset = await _datasetService.GetByIdAsync(id);

        if (dataset == null)
        {
            return NotFound(new
            {
                message = "Dataset introuvable."
            });
        }

        var query = dataset.Rows.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(row =>
                row.Values.Any(value =>
                    value.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        var total = query.Count();

        var rows = query
            .Skip(Math.Max(skip, 0))
            .Take(Math.Clamp(take, 1, 1000))
            .ToList();

        return Ok(new
        {
            dataset.Id,
            dataset.Name,
            dataset.Columns,
            Total = total,
            Skip = skip,
            Take = take,
            Rows = rows
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _datasetService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Dataset introuvable."
            });
        }

        return NoContent();
    }
}