namespace CSVReader.Controllers;

using Microsoft.AspNetCore.Mvc;
using CSVReader.Models;
using CSVReader.Services;

[ApiController]
[Route("api/[controller]")]
public class CsvController(CsvStorageService storage) : ControllerBase
{
    [HttpPost] // Annotation pour requête POST (api)
    [RequestSizeLimit(10 * 1024 * 1024)] // Limite de taille du fichier
    // Upload d'un fichier CSV
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file.Length == 0)                return BadRequest("Fichier vide.");
        if (!file.FileName.EndsWith(".csv")) return BadRequest("Format CSV requis.");

        try
        {
            var saved = await storage.SaveAsync(file);
            return CreatedAtAction(nameof(Get), new { id = saved.Id }, saved);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("/createEP")] // Annotation pour requête POST (api/createEP)
    // Upload d'un fichier CSV pour ouvrir un endpoint
    public async Task<IActionResult> UploadForEndpoint(IFormFile file)
    {
        if (file.Length == 0)                return BadRequest("Fichier vide.");
        if (!file.FileName.EndsWith(".csv")) return BadRequest("Format CSV requis.");

        try
        {
            var saved = await storage.SaveAsync(file);

            return CreatedAtAction(nameof(Get), new { id = saved.Id }, saved);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpGet] // Annotation pour requête GET (api)
    // Retourner tous les fichiers enregistrés
    public async Task<IActionResult> GetAll() => Ok(await storage.GetAllAsync());

    [HttpGet("{id}")] // Annotation pour requête GET (api/{id})
    // Retourner un fichier via son identifiant
    public async Task<IActionResult> Get(int id)
    {
        var file = await storage.GetAsync(id);
        return file is null ? NotFound() : Ok(file);
    }

    [HttpGet("/createEP/{name}")] // Annotation pour requête GET (api/createEP/{name})
    // Retourner un fichier via son nom
    public async Task<IActionResult> GetByName(string name)
    {
        var file = await storage.GetByNameAsync(name);
        return file is null ? NotFound() : Ok(file);
    }

    [HttpDelete("{id:int}")] // Annotation pour requête DELETE (api/{id})
    // Supprimer un fichier via son identifiant
    public async Task<IActionResult> Delete(int id) =>
        await storage.DeleteAsync(id) ? NoContent() : NotFound();

    [HttpDelete("{name}")] // Annotation pour requête DELETE (api/{name})
    // Supprimer un fichier via son nom
    public async Task<IActionResult> DeleteByName(string name) =>
        await storage.DeleteByNameAsync(name) ? NoContent() : NotFound();

}