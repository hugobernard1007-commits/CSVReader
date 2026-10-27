using CsvApi.Backend.Models;
using CsvApi.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CsvApi.Backend.Controllers;

[ApiController]
[Route("api/comparisons")]
public class ComparisonController : ControllerBase
{
    private readonly IComparisonService _comparisonService;

    public ComparisonController(IComparisonService comparisonService)
    {
        _comparisonService = comparisonService;
    }

    [HttpPost]
    public async Task<ActionResult<CompareResponse>> Compare([FromBody] CompareRequest request)
    {
        try
        {
            var response = await _comparisonService.CompareAsync(request);
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
}
