using CsvApi.Backend.Models;

namespace CsvApi.Backend.Services;

public interface IComparisonService
{
    Task<CompareResponse> CompareAsync(CompareRequest request);
}