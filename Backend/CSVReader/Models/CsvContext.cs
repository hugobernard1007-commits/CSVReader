using Microsoft.EntityFrameworkCore;

namespace CSVReader.Models;

public class CsvContext : DbContext
{
    public CsvContext(DbContextOptions<CsvContext> options)
        : base(options)
    {
    }

    public DbSet<CsvFile> CsvFiles { get; set; } = null!;
}