namespace CsvApi.Backend.Models;

public class CsvDataset
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public List<string> Columns { get; set; } = new();

    public List<Dictionary<string, string>> Rows { get; set; } = new();
}
