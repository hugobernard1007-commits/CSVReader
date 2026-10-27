using CsvApi.Backend.Models;

namespace CsvApi.Backend.Services;

public interface IDynamicEndpointService
{
    Task<List<DynamicEndpoint>> GetAllAsync();

    Task<DynamicEndpoint?> GetBySlugAsync(string slug);

    Task<DynamicEndpoint> CreateAsync(CreateEndpointRequest request);

    Task<bool> DeleteAsync(Guid id);
}