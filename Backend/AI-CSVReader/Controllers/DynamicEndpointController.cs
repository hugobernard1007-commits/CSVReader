using CsvApi.Backend.Models;
using CsvApi.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CsvApi.Backend.Controllers;

[ApiController]
[Route("api/endpoints")]
public class DynamicEndpointsController : ControllerBase
{
    private readonly IDynamicEndpointService _endpointService;

    public DynamicEndpointsController(IDynamicEndpointService endpointService)
    {
        _endpointService = endpointService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DynamicEndpoint>>> GetAll()
    {
        var endpoints = await _endpointService.GetAllAsync();
        return Ok(endpoints);
    }

    [HttpPost]
    public async Task<ActionResult<DynamicEndpoint>> Create([FromBody] CreateEndpointRequest request)
    {
        try
        {
            var endpoint = await _endpointService.CreateAsync(request);

            return Created(endpoint.Url, endpoint);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _endpointService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Endpoint introuvable."
            });
        }

        return NoContent();
    }
}