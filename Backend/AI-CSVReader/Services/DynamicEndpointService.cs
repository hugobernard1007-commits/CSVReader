using System.Text.RegularExpressions;
using CsvApi.Backend.Models;
using CsvApi.Backend.Storage;

namespace CsvApi.Backend.Services;

public partial class DynamicEndpointService : IDynamicEndpointService
{
    private readonly JsonFileStorage _storage;
    private readonly ICsvDatasetService _datasetService;

    public DynamicEndpointService(
        JsonFileStorage storage,
        ICsvDatasetService datasetService)
    {
        _storage = storage;
        _datasetService = datasetService;
    }

    public async Task<List<DynamicEndpoint>> GetAllAsync()
    {
        return await _storage.LoadEndpointsAsync();
    }

    public async Task<DynamicEndpoint?> GetBySlugAsync(string slug)
    {
        var endpoints = await _storage.LoadEndpointsAsync();

        return endpoints.FirstOrDefault(endpoint =>
            endpoint.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<DynamicEndpoint> CreateAsync(CreateEndpointRequest request)
    {
        ValidateRequest(request);

        var dataset = await _datasetService.GetByIdAsync(request.DatasetId);

        if (dataset == null)
        {
            throw new InvalidOperationException("Le dataset associé à l'endpoint est introuvable.");
        }

        var slug = Slugify(request.Slug);

        var endpoints = await _storage.LoadEndpointsAsync();

        if (endpoints.Any(endpoint => endpoint.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Un endpoint avec ce slug existe déjà.");
        }

        var invalidColumns = request.Columns
            .Where(column => !dataset.Columns.Contains(column))
            .ToList();

        if (invalidColumns.Count > 0)
        {
            throw new InvalidOperationException(
                $"Colonnes invalides pour ce dataset : {string.Join(", ", invalidColumns)}"
            );
        }

        var endpoint = new DynamicEndpoint
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug,
            DatasetId = request.DatasetId,
            Columns = request.Columns.Distinct().ToList(),
            Description = request.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        endpoints.Add(endpoint);

        await _storage.SaveEndpointsAsync(endpoints);

        return endpoint;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var endpoints = await _storage.LoadEndpointsAsync();
        var endpoint = endpoints.FirstOrDefault(item => item.Id == id);

        if (endpoint == null)
        {
            return false;
        }

        endpoints.Remove(endpoint);
        await _storage.SaveEndpointsAsync(endpoints);

        return true;
    }

    private static void ValidateRequest(CreateEndpointRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("Le nom de l'endpoint est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(request.Slug))
        {
            throw new InvalidOperationException("Le slug de l'endpoint est obligatoire.");
        }

        if (request.DatasetId == Guid.Empty)
        {
            throw new InvalidOperationException("Le dataset est obligatoire.");
        }

        if (request.Columns == null || request.Columns.Count == 0)
        {
            throw new InvalidOperationException("Au moins une colonne doit être exposée.");
        }
    }

    private static string Slugify(string value)
    {
        var slug = value.Trim().ToLowerInvariant();
        slug = SlugRegex().Replace(slug, "-");
        slug = slug.Trim('-');

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new InvalidOperationException("Le slug généré est invalide.");
        }

        return slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex SlugRegex();
}