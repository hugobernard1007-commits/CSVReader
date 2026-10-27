namespace CsvApi.Backend.Models;

public class CompareResponse
{
    public Guid FirstDatasetId { get; set; }

    public Guid SecondDatasetId { get; set; }

    public string KeyColumn { get; set; } = string.Empty;

    public int AddedCount { get; set; }

    public int RemovedCount { get; set; }

    public int ModifiedCount { get; set; }

    public int TotalDifferences { get; set; }

    public List<DifferenceRow> Differences { get; set; } = new();
}