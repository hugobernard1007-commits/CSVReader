using CsvApi.Backend.Services;
using CsvApi.Backend.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Dev", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});


builder.Services.AddSingleton<JsonFileStorage>();

builder.Services.AddScoped<ICsvParserService, CsvParserService>();
builder.Services.AddScoped<ICsvDatasetService, CsvDatasetService>();
builder.Services.AddScoped<IComparisonService, ComparisonService>();
builder.Services.AddScoped<IDynamicEndpointService, DynamicEndpointService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();


}
app.UseCors("Dev");
app.UseHttpsRedirection();

app.MapControllers();


app.Run();