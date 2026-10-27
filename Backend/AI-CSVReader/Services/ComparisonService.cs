using System.Text.Json;
using CsvApi.Backend.Models;

namespace CsvApi.Backend.Services;

public class ComparisonService : IComparisonService
{
    private readonly ICsvDatasetService _datasetService;

    public ComparisonService(ICsvDatasetService datasetService)
    {
        _datasetService = datasetService;
    }

    public async Task<CompareResponse> CompareAsync(CompareRequest request)
    {
        var firstDataset = await _datasetService.GetByIdAsync(request.FirstDatasetId);
        var secondDataset = await _datasetService.GetByIdAsync(request.SecondDatasetId);

        if (firstDataset == null)
        {
            throw new InvalidOperationException("Le premier dataset est introuvable.");
        }

        if (secondDataset == null)
        {
            throw new InvalidOperationException("Le deuxième dataset est introuvable.");
        }

        var keyColumn = string.IsNullOrWhiteSpace(request.KeyColumn)
            ? "__index"
            : request.KeyColumn.Trim();

        var columnsToCompare = ResolveColumns(firstDataset, secondDataset, request.Columns, keyColumn);

        var firstMap = CreateRowMap(firstDataset.Rows, keyColumn);
        var secondMap = CreateRowMap(secondDataset.Rows, keyColumn);

        var differences = new List<DifferenceRow>();

        foreach (var firstItem in firstMap)
        {
            var key = firstItem.Key;
            var firstRow = firstItem.Value;

            if (!secondMap.TryGetValue(key, out var secondRow))
            {
                differences.Add(new DifferenceRow
                {
                    Status = "Supprimé",
                    Key = key,
                    Column = "*",
                    OldValue = SerializeRow(firstRow),
                    NewValue = string.Empty
                });

                continue;
            }

            foreach (var column in columnsToCompare)
            {
                var oldValue = GetValue(firstRow, column);
                var newValue = GetValue(secondRow, column);

                if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
                {
                    differences.Add(new DifferenceRow
                    {
                        Status = "Modifié",
                        Key = key,
                        Column = column,
                        OldValue = oldValue,
                        NewValue = newValue
                    });
                }
            }
        }

        foreach (var secondItem in secondMap)
        {
            var key = secondItem.Key;

            if (!firstMap.ContainsKey(key))
            {
                differences.Add(new DifferenceRow
                {
                    Status = "Ajouté",
                    Key = key,
                    Column = "*",
                    OldValue = string.Empty,
                    NewValue = SerializeRow(secondItem.Value)
                });
            }
        }

        return new CompareResponse
        {
            FirstDatasetId = firstDataset.Id,
            SecondDatasetId = secondDataset.Id,
            KeyColumn = keyColumn,
            Differences = differences,
            AddedCount = differences.Count(item => item.Status == "Ajouté"),
            RemovedCount = differences.Count(item => item.Status == "Supprimé"),
            ModifiedCount = differences.Count(item => item.Status == "Modifié"),
            TotalDifferences = differences.Count
        };
    }

    private static List<string> ResolveColumns(
        CsvDataset firstDataset,
        CsvDataset secondDataset,
        List<string>? requestedColumns,
        string keyColumn)
    {
        var commonColumns = firstDataset.Columns
            .Intersect(secondDataset.Columns)
            .Where(column => column != keyColumn)
            .ToList();

        if (requestedColumns == null || requestedColumns.Count == 0)
        {
            return commonColumns;
        }

        return requestedColumns
            .Where(column => commonColumns.Contains(column))
            .Where(column => column != keyColumn)
            .Distinct()
            .ToList();
    }

    private static Dictionary<string, Dictionary<string, string>> CreateRowMap(
        List<Dictionary<string, string>> rows,
        string keyColumn)
    {
        var map = new Dictionary<string, Dictionary<string, string>>();

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];

            var key = keyColumn == "__index"
                ? (index + 1).ToString()
                : GetValue(row, keyColumn);

            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            map[key] = row;
        }

        return map;
    }

    private static string GetValue(Dictionary<string, string> row, string column)
    {
        return row.TryGetValue(column, out var value)
            ? value?.Trim() ?? string.Empty
            : string.Empty;
    }

    private static string SerializeRow(Dictionary<string, string> row)
    {
        return JsonSerializer.Serialize(row);
    }
}