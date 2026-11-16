using System.Globalization;

namespace ExpenseGuided;

public static class ConsoleInput
{
    public static bool TryReadNonNegativeAmount(TextReader reader, TextWriter writer, out decimal amount)
        => TryReadNonNegativeAmount(reader, writer, out amount, out _);

    public static bool TryReadNonNegativeAmount(TextReader reader, TextWriter writer, out decimal amount, out bool endOfInput)
    {
        string? text = reader.ReadLine();
        if (text is null)
        {
            writer.WriteLine("Đã kết thúc input.");
            amount = 0;
            endOfInput = true;
            return false;
        }

        const NumberStyles plainMoney = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite |
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
        if (decimal.TryParse(text, plainMoney, CultureInfo.InvariantCulture, out amount) && amount >= 0)
        {
            endOfInput = false;
            return true;
        }

        writer.WriteLine("Hãy nhập một số tiền không âm.");
        amount = 0;
        endOfInput = false;
        return false;
    }
}
