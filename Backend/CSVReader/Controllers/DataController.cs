namespace CSVReader.Controllers;

using Microsoft.AspNetCore.Mvc;
using CSVReader.Models;
using CSVReader.Services;


[ApiController]
[Route("api/[controller]")]
public class DataController(CsvStorageService storage) : ControllerBase
{
    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetByName(string fileName)
    {
        var all  = await storage.GetAllAsync();
        var meta = all.FirstOrDefault(f =>
            string.Equals(f.Name, fileName, StringComparison.OrdinalIgnoreCase));

        if (meta is null) return NotFound($"Fichier '{fileName}' introuvable.");

        var file = await storage.GetAsync(meta.Id);
        if (file is null) return NotFound();

        return Ok(new { file.Headers, file.Rows });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await storage.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var file = await storage.GetAsync(id);
        if (file is null) return NotFound($"Fichier avec l'ID '{id}' introuvable.");

        return Ok(new { file });
    }
}