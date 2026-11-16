using System.Globalization;

namespace ExpenseGuided;

public static class ConsoleRunner
{
    public static void Run(TextReader reader, TextWriter writer)
    {
        var service = new ExpenseService();
        writer.WriteLine("Sổ chi tiêu guided reference");
        writer.WriteLine("Nhập 3 khoản; input sai bị từ chối và không cộng vào tổng.");

        for (int index = 1; index <= 3; index++)
        {
            string category;
            while (true)
            {
                writer.Write($"Khoản {index} danh mục: ");
                string? categoryInput = reader.ReadLine();
                if (categoryInput is null)
                {
                    writer.WriteLine("Đã kết thúc input.");
                    return;
                }
                if (!string.IsNullOrWhiteSpace(categoryInput))
                {
                    category = categoryInput;
                    break;
                }
                writer.WriteLine("Lỗi: Danh mục không được rỗng.");
            }

            while (true)
            {
                writer.Write($"Khoản {index} số tiền: ");
                if (!ConsoleInput.TryReadNonNegativeAmount(reader, writer, out decimal amount, out bool eof))
                {
                    if (eof) return;
                    continue;
                }

                service.Add(category, amount);
                writer.WriteLine($"Đã thêm: {category.Trim()} = {amount.ToString(CultureInfo.InvariantCulture)}");
                break;
            }
        }

        writer.WriteLine($"Tổng: {service.Total().ToString(CultureInfo.InvariantCulture)}");
    }
}
