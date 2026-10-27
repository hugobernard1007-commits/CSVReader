namespace CsvApi.Backend.Models;

public class DynamicEndpoint
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public Guid DatasetId { get; set; }

    public List<string> Columns { get; set; } = new();

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Url => $"/api/public/{Slug}";
}