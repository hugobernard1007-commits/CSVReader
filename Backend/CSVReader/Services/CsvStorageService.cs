namespace CSVReader.Services;

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using CSVReader.Models;
public class CsvStorageService : ICsvStorageService
{
    private readonly string _dataDir;
    private readonly string _indexPath;
    private readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    // Configuration du service
    public CsvStorageService(IConfiguration config)
    {
        _dataDir   = config["CsvStorage:Path"] ?? "data";
        _indexPath = Path.Combine(_dataDir, "index.json");
        Directory.CreateDirectory(_dataDir);
    }

    // ── Index ──────────────────────────────────────────────

    // Lire l'index
    private async Task<List<CsvFile>> ReadIndexAsync()
    {
        if (!File.Exists(_indexPath)) return [];
        var json = await File.ReadAllTextAsync(_indexPath);
        return JsonSerializer.Deserialize<List<CsvFile>>(json, _json) ?? [];
    }

    // Ecrire l'index
    private async Task WriteIndexAsync(List<CsvFile> index) =>
        await File.WriteAllTextAsync(_indexPath,
            JsonSerializer.Serialize(index.Select(f => new CsvFile
            {
                Id               = f.Id,
                Name             = f.Name,
                OriginalFileName = f.OriginalFileName,
                UploadedAt       = f.UploadedAt,
                SizeBytes        = f.SizeBytes
            }).ToList(), _json));

    // ── CRUD ───────────────────────────────────────────────

    // Sauvegarder un fichier en JSON
    public async Task<CsvFile> SaveAsync(IFormFile file)
    {
        var index = await ReadIndexAsync();
        var id    = index.Count > 0 ? index.Max(f => f.Id) + 1 : 1;

        // Parser le CSV en mémoire
        using var reader = new StreamReader(file.OpenReadStream());
        var raw     = await reader.ReadToEndAsync();
        var lines   = raw.Split('\n')
                         .Select(l => l.Trim())
                         .Where(l => !string.IsNullOrEmpty(l))
                         .ToList();

        var headers = SplitCsvLine(lines[0]);
        var rows    = lines.Skip(1).Select(line =>
        {
            var values = SplitCsvLine(line);
            return headers
                .Select((h, i) => (h, v: i < values.Count ? values[i] : ""))
                .ToDictionary(x => x.h, x => x.v );
        }).ToList();

        var csvFile = new CsvFile
        {
            Id               = id,
            Name             = Path.GetFileNameWithoutExtension(file.FileName),
            OriginalFileName = file.FileName,
            SizeBytes        = file.Length,
            Headers          = headers,
            Rows             = rows
        };

        // Écrire le fichier JSON
        var filePath = Path.Combine(_dataDir, $"{id}.json");
        await File.WriteAllTextAsync(filePath,
            JsonSerializer.Serialize(csvFile, _json));

        // Mettre à jour l'index
        index.Add(csvFile);
        await WriteIndexAsync(index);

        return csvFile;
    }

    // Retourner tous les fichiers enregistrés
    public async Task<List<CsvFile>> GetAllAsync() => await ReadIndexAsync();

    // Retourner un fichier spécifique via son identifiant
    public async Task<CsvFile?> GetAsync(int id)
    {
        var filePath = Path.Combine(_dataDir, $"{id}.json");
        if (!File.Exists(filePath)) return null;
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<CsvFile>(json, _json);
    }

    // Retourner un fichier spécifique via son nom
    public async Task<CsvFile?> GetByNameAsync(string name)
    {
        var index = await ReadIndexAsync();
        var file  = index.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (file is null) return null;
        return await GetAsync(file.Id);
    }

    // Supprimer un fichier spécifique via son identifiant
    public async Task<bool> DeleteAsync(int id)
    {
        var filePath = Path.Combine(_dataDir, $"{id}.json");
        if (!File.Exists(filePath)) return false;

        File.Delete(filePath);

        var index = await ReadIndexAsync();
        index.RemoveAll(f => f.Id == id);
        await WriteIndexAsync(index);
        return true;
    }

    // Supprimer un fichier spécifique via son nom
    public async Task<bool> DeleteByNameAsync(string name)
    {
        var index = await ReadIndexAsync();
        var file  = index.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (file is null) return false;

        var filePath = Path.Combine(_dataDir, $"{file.Id}.json");
        if (File.Exists(filePath)) File.Delete(filePath);

        index.RemoveAll(f => f.Id == file.Id);
        await WriteIndexAsync(index);
        return true;
    }

    // ── CSV parser ─────────────────────────────────────────

    // Couper les lignes du fichier CSV
    private static List<string> SplitCsvLine(string line)
    {
        var result   = new List<string>();
        var current  = new System.Text.StringBuilder();
        var inQuotes = false;

        foreach (var ch in line)
        {
            if (ch == '"')        inQuotes = !inQuotes;
            else if (ch == ',' && !inQuotes) { result.Add(current.ToString().Trim()); current.Clear(); }
            else                  current.Append(ch);
        }

        result.Add(current.ToString().Trim());
        return result;
    }
}