using System.Text.Json;

namespace ExpenseGuided;

public sealed class DatedJsonExpenseStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public void Save(string path, IReadOnlyCollection<DatedExpense> expenses)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        File.WriteAllText(path, JsonSerializer.Serialize(expenses, Options));
    }

    public IReadOnlyList<DatedExpense> Load(string path)
    {
        if (!File.Exists(path)) return [];
        string json = File.ReadAllText(path);
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
                throw new JsonException("JSON phải có root array.");

            var result = new List<DatedExpense>();
            var ids = new HashSet<int>();
            foreach (JsonElement element in document.RootElement.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object) throw new JsonException("Phần tử phải là object.");
                Require(element, "Id", JsonValueKind.Number);
                Require(element, "Category", JsonValueKind.String);
                Require(element, "Amount", JsonValueKind.Number);
                Require(element, "SpentOn", JsonValueKind.String);
                int id = element.GetProperty("Id").GetInt32();
                if (!ids.Add(id)) throw new JsonException("Id bị trùng.");
                string? category = element.GetProperty("Category").GetString();
                decimal amount = element.GetProperty("Amount").GetDecimal();
                DateOnly spentOn = element.GetProperty("SpentOn").Deserialize<DateOnly>()!;
                result.Add(new DatedExpense(id, category ?? string.Empty, amount, spentOn));
            }
            return result;
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("JSON không hợp lệ; file được giữ nguyên để kiểm tra.", error);
        }
        catch (FormatException error)
        {
            throw new InvalidDataException("Ngày lưu trữ không hợp lệ; file được giữ nguyên.", error);
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException("Dữ liệu lưu trữ vi phạm invariant; file được giữ nguyên.", error);
        }
    }

    private static void Require(JsonElement element, string name, JsonValueKind kind)
    {
        if (!element.TryGetProperty(name, out JsonElement property) || property.ValueKind != kind)
            throw new JsonException($"Thiếu hoặc sai kiểu trường {name}.");
    }
}
