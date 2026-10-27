namespace CsvApi.Backend.Models;

public class CsvUploadResponse
{
    public Guid DatasetId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public int RowCount { get; set; }

    public int ColumnCount { get; set; }

    public List<string> Columns { get; set; } = new();
}