using CsvApi.Backend.Models;

namespace CsvApi.Backend.Services;

public interface ICsvParserService
{
    Task<CsvDataset> ParseAsync(IFormFile file);
}