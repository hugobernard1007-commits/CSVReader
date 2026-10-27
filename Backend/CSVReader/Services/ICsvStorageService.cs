using CSVReader.Models;
namespace CSVReader.Services;
public interface ICsvStorageService
{
    // Sauvegarder un fichier en JSON
    Task<CsvFile> SaveAsync(IFormFile file);

    // Retourner tous les fichiers enregistrés
    Task<List<CsvFile>> GetAllAsync();

    // Retourner un fichier spécifique via son identifiant
    Task<CsvFile> GetAsync(int id);

    // Retourner un fichier spécifique via son nom
    Task<CsvFile> GetByNameAsync(string name);

    // Supprimer un fichier spécifique via son identifiant
    Task<bool> DeleteAsync(int id);

    // Supprimer un fichier spécifique via son nom
    Task<bool> DeleteByNameAsync(string name);
}