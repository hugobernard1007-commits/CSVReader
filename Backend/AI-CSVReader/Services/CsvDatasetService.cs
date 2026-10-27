using CsvApi.Backend.Models;
using CsvApi.Backend.Storage;

namespace CsvApi.Backend.Services;

public class CsvDatasetService : ICsvDatasetService
{
    private readonly ICsvParserService _parserService;
    private readonly JsonFileStorage _storage;

    public CsvDatasetService(
        ICsvParserService parserService,
        JsonFileStorage storage)
    {
        _parserService = parserService;
        _storage = storage;
    }

    public async Task<CsvDataset> UploadAsync(IFormFile file)
    {
        var dataset = await _parserService.ParseAsync(file);

        var datasets = await _storage.LoadDatasetsAsync();
        datasets.Add(dataset);

        await _storage.SaveDatasetsAsync(datasets);

        return dataset;
    }

    public async Task<List<CsvDataset>> GetAllAsync()
    {
        return await _storage.LoadDatasetsAsync();
    }

    public async Task<CsvDataset?> GetByIdAsync(Guid id)
    {
        var datasets = await _storage.LoadDatasetsAsync();
        return datasets.FirstOrDefault(dataset => dataset.Id == id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var datasets = await _storage.LoadDatasetsAsync();
        var dataset = datasets.FirstOrDefault(item => item.Id == id);

        if (dataset == null)
        {
            return false;
        }

        datasets.Remove(dataset);
        await _storage.SaveDatasetsAsync(datasets);

        return true;
    }
}