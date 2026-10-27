namespace CsvApi.Backend.Models;

public class CompareRequest
{
    public Guid FirstDatasetId { get; set; }

    public Guid SecondDatasetId { get; set; }

    public string? KeyColumn { get; set; }

    public List<string>? Columns { get; set; }
}