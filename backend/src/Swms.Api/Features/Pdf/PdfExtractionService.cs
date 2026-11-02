using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Exceptions;

namespace Swms.Api.Features.Pdf;

public record ExtractedPdfPage(
    int PageNumber,
    string Text,
    bool HasTextLayer,
    int CharCount,
    int WordCount
);

public record PdfExtractionResult(
    bool Success,
    int TotalPages,
    IReadOnlyList<ExtractedPdfPage> Pages,
    string? ErrorMessage = null,
    bool IsEncrypted = false,
    bool IsCorrupt = false
);

public interface IPdfExtractionService
{
    PdfExtractionResult Extract(byte[] pdfBytes);
    PdfExtractionResult Extract(Stream stream);
    PdfExtractionResult Extract(string filePath);
}

public class PdfExtractionService : IPdfExtractionService
{
    private static readonly byte[] PdfHeader = "%PDF-"u8.ToArray();

    public PdfExtractionResult Extract(byte[] pdfBytes)
    {
        if (pdfBytes == null || pdfBytes.Length < 5)
        {
            return new PdfExtractionResult(
                Success: false,
                TotalPages: 0,
                Pages: Array.Empty<ExtractedPdfPage>(),
                ErrorMessage: "Tệp không phải định dạng PDF hợp lệ hoặc kích thước quá nhỏ.",
                IsCorrupt: true);
        }

        var headerIndex = pdfBytes.AsSpan().IndexOf(PdfHeader);
        if (headerIndex < 0 || headerIndex > 1024)
        {
            return new PdfExtractionResult(
                Success: false,
                TotalPages: 0,
                Pages: Array.Empty<ExtractedPdfPage>(),
                ErrorMessage: "Tệp không chứa tiêu đề định dạng PDF chuẩn (%PDF-).",
                IsCorrupt: true);
        }

        try
        {
            using var pdfDoc = PdfDocument.Open(pdfBytes);
            return ProcessDocument(pdfDoc);
        }
        catch (PdfDocumentEncryptedException)
        {
            return new PdfExtractionResult(
                Success: false,
                TotalPages: 0,
                Pages: Array.Empty<ExtractedPdfPage>(),
                ErrorMessage: "Tệp PDF đã được đặt mật khẩu hoặc mã hóa. Vui lòng mở khóa tệp trước khi tải lên.",
                IsEncrypted: true);
        }
        catch (Exception ex)
        {
            return new PdfExtractionResult(
                Success: false,
                TotalPages: 0,
                Pages: Array.Empty<ExtractedPdfPage>(),
                ErrorMessage: $"Tệp PDF bị hỏng hoặc không đúng định dạng chuẩn: {ex.Message}",
                IsCorrupt: true);
        }
    }

    public PdfExtractionResult Extract(Stream stream)
    {
        if (stream == null)
        {
            return new PdfExtractionResult(
                Success: false,
                TotalPages: 0,
                Pages: Array.Empty<ExtractedPdfPage>(),
                ErrorMessage: "Dữ liệu luồng PDF không hợp lệ.",
                IsCorrupt: true);
        }

        using var ms = new MemoryStream();
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }
        stream.CopyTo(ms);
        return Extract(ms.ToArray());
    }

    public PdfExtractionResult Extract(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return new PdfExtractionResult(
                Success: false,
                TotalPages: 0,
                Pages: Array.Empty<ExtractedPdfPage>(),
                ErrorMessage: "Đường dẫn tệp PDF không tồn tại.",
                IsCorrupt: true);
        }

        try
        {
            var bytes = File.ReadAllBytes(filePath);
            return Extract(bytes);
        }
        catch (Exception ex)
        {
            return new PdfExtractionResult(
                Success: false,
                TotalPages: 0,
                Pages: Array.Empty<ExtractedPdfPage>(),
                ErrorMessage: $"Không thể đọc tệp PDF: {ex.Message}",
                IsCorrupt: true);
        }
    }

    private static PdfExtractionResult ProcessDocument(PdfDocument pdfDoc)
    {
        if (pdfDoc.NumberOfPages <= 0)
        {
            return new PdfExtractionResult(
                Success: false,
                TotalPages: 0,
                Pages: Array.Empty<ExtractedPdfPage>(),
                ErrorMessage: "Tệp PDF không chứa trang nào hợp lệ.",
                IsCorrupt: true);
        }

        var pages = new List<ExtractedPdfPage>(pdfDoc.NumberOfPages);
        for (int i = 1; i <= pdfDoc.NumberOfPages; i++)
        {
            var page = pdfDoc.GetPage(i);
            var rawText = page.Text ?? string.Empty;
            var normalizedText = rawText.Normalize(NormalizationForm.FormC);
            var hasText = !string.IsNullOrWhiteSpace(normalizedText);
            var charCount = normalizedText.Length;
            var words = hasText ? normalizedText.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
            var wordCount = words.Length;

            pages.Add(new ExtractedPdfPage(
                PageNumber: i,
                Text: normalizedText,
                HasTextLayer: hasText,
                CharCount: charCount,
                WordCount: wordCount
            ));
        }

        return new PdfExtractionResult(
            Success: true,
            TotalPages: pdfDoc.NumberOfPages,
            Pages: pages
        );
    }
}
