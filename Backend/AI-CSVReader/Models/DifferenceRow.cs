namespace CsvApi.Backend.Models;

public class DifferenceRow
{
    public string Status { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string Column { get; set; } = string.Empty;

    public string OldValue { get; set; } = string.Empty;

    public string NewValue { get; set; } = string.Empty;
}