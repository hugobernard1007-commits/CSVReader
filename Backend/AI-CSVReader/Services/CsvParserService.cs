using System.Globalization;
using CsvApi.Backend.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace CsvApi.Backend.Services;

public class CsvParserService : ICsvParserService
{
    private static readonly string[] AllowedExtensions = [".csv"];
    private const long MaxFileSizeInBytes = 20 * 1024 * 1024;

    public async Task<CsvDataset> ParseAsync(IFormFile file)
    {
        ValidateFile(file);

        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            DetectDelimiter = true,
            BadDataFound = null,
            MissingFieldFound = null,
            HeaderValidated = null,
            TrimOptions = TrimOptions.Trim
        };

        using var csv = new CsvReader(reader, config);

        await csv.ReadAsync();
        csv.ReadHeader();

        var headers = csv.HeaderRecord?
            .Where(header => !string.IsNullOrWhiteSpace(header))
            .Distinct()
            .ToList() ?? new List<string>();

        if (headers.Count == 0)
        {
            throw new InvalidOperationException("Le fichier CSV ne contient aucun en-tête valide.");
        }

        var rows = new List<Dictionary<string, string>>();

        while (await csv.ReadAsync())
        {
            var row = new Dictionary<string, string>();

            foreach (var header in headers)
            {
                row[header] = csv.GetField(header) ?? string.Empty;
            }

            if (row.Values.Any(value => !string.IsNullOrWhiteSpace(value)))
            {
                rows.Add(row);
            }
        }

        if (rows.Count == 0)
        {
            throw new InvalidOperationException("Le fichier CSV ne contient aucune ligne exploitable.");
        }

        return new CsvDataset
        {
            Id = Guid.NewGuid(),
            Name = Path.GetFileNameWithoutExtension(file.FileName),
            OriginalFileName = Path.GetFileName(file.FileName),
            UploadedAt = DateTime.UtcNow,
            Columns = headers,
            Rows = rows
        };
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new InvalidOperationException("Aucun fichier n'a été transmis.");
        }

        if (file.Length > MaxFileSizeInBytes)
        {
            throw new InvalidOperationException("Le fichier dépasse la taille maximale autorisée de 20 Mo.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Seuls les fichiers .csv sont autorisés.");
        }
    }
}