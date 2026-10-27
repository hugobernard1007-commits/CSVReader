
namespace CSVReader.Models;

public class CsvFile
{
    // Identifiant du fichier
    public int Id { get; set; }
    
    // Nom du fichier sans l'extension
    public string Name { get; set; } = string.Empty;

    // Nom du fichier entier
    public string OriginalFileName { get; set; } = string.Empty;

    // Date d'upload du fichier
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Taille du fichier (en bytes)
    public long SizeBytes { get; set; }

    // Entêtes du fichier
    public List<string> Headers { get; set; } = [];

    // Colonnes du fichier (hors entêtes)
    public List<Dictionary<string, string>> Rows { get; set; } = [];
}