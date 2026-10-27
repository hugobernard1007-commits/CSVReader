using System.Text.Json;
using CsvApi.Backend.Models;

namespace CsvApi.Backend.Storage;

public class JsonFileStorage
{
    private readonly string _storageFolder;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonFileStorage(IWebHostEnvironment environment)
    {
        _storageFolder = Path.Combine(environment.ContentRootPath, "App_Data");

        if (!Directory.Exists(_storageFolder))
        {
            Directory.CreateDirectory(_storageFolder);
        }

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<CsvDataset>> LoadDatasetsAsync()
    {
        return await LoadAsync<CsvDataset>("datasets.json");
    }

    public async Task SaveDatasetsAsync(List<CsvDataset> datasets)
    {
        await SaveAsync("datasets.json", datasets);
    }

    public async Task<List<DynamicEndpoint>> LoadEndpointsAsync()
    {
        return await LoadAsync<DynamicEndpoint>("endpoints.json");
    }

    public async Task SaveEndpointsAsync(List<DynamicEndpoint> endpoints)
    {
        await SaveAsync("endpoints.json", endpoints);
    }

    private async Task<List<T>> LoadAsync<T>(string fileName)
    {
        var path = Path.Combine(_storageFolder, fileName);

        if (!File.Exists(path))
        {
            return new List<T>();
        }

        var json = await File.ReadAllTextAsync(path);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<T>();
        }

        return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
    }

    private async Task SaveAsync<T>(string fileName, List<T> data)
    {
        var path = Path.Combine(_storageFolder, fileName);
        var json = JsonSerializer.Serialize(data, _jsonOptions);

        await File.WriteAllTextAsync(path, json);
    }
}