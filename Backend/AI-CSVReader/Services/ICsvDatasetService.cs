using CsvApi.Backend.Models;

namespace CsvApi.Backend.Services;

public interface ICsvDatasetService
{
    Task<CsvDataset> UploadAsync(IFormFile file);

    Task<List<CsvDataset>> GetAllAsync();

    Task<CsvDataset?> GetByIdAsync(Guid id);

    Task<bool> DeleteAsync(Guid id);
}