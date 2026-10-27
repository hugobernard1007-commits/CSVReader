using Microsoft.EntityFrameworkCore;
using CSVReader.Models;
using CSVReader.Services;

var builder = WebApplication.CreateBuilder(args);

// Ajout des services au conteneur
builder.Services.AddScoped<CsvStorageService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<CsvContext>(opt =>
    opt.UseInMemoryDatabase("CsvFiles"));
builder.Services.AddCors(options =>
{
    options.AddPolicy("Dev", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}
var dataPath = app.Configuration["CsvStorage:Path"] ?? "data";
if (Directory.Exists(dataPath))
{
    Directory.Delete(dataPath, recursive: true);
}
Directory.CreateDirectory(dataPath);

app.UseCors("Dev");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
