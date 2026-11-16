using System.Text.Json;

namespace ExpenseGuided;

public sealed class JsonExpenseStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public void Save(string path, IReadOnlyCollection<Expense> expenses)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        File.WriteAllText(path, JsonSerializer.Serialize(expenses, Options));
    }

    public IReadOnlyList<Expense> Load(string path)
    {
        if (!File.Exists(path)) return [];
        string json = File.ReadAllText(path);
        try
        {
            List<ExpenseDto>? values = JsonSerializer.Deserialize<List<ExpenseDto>>(json, Options);
            if (values is null) throw new JsonException("JSON không chứa danh sách.");
            if (values.Any(value => value is null))
                throw new JsonException("JSON chứa phần tử null.");
            return values.Select(value => new Expense(value!.Category ?? "", value.Amount)).ToList();
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("JSON không hợp lệ; file được giữ nguyên để kiểm tra.", error);
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException("Dữ liệu lưu trữ vi phạm invariant Expense; file được giữ nguyên.", error);
        }
    }

    private sealed record ExpenseDto(string? Category, decimal Amount);
}
