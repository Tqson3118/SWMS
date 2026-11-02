using System.Text;
using Swms.Api.Features.Pdf;
using Xunit;

namespace Swms.Tests;

public class PdfExtractionServiceTests
{
    private readonly PdfExtractionService _service = new();

    private static string GetRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            if (Directory.Exists(Path.Combine(dir, "test-fixtures")) && Directory.Exists(Path.Combine(dir, "test")))
            {
                return dir;
            }
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        throw new DirectoryNotFoundException("Could not find repository root containing 'test-fixtures' and 'test'.");
    }

    [Fact]
    public void Small3PagePdf_ExtractsSuccessfully_TotalPages3_AllPagesHaveTextLayer()
    {
        var path = Path.Combine(GetRepoRoot(), "test-fixtures", "small-3p.pdf");
        Assert.True(File.Exists(path), $"Fixture small-3p.pdf should exist at {path}");

        var bytes = File.ReadAllBytes(path);
        var result = _service.Extract(bytes);

        Assert.True(result.Success);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(3, result.Pages.Count);

        for (int i = 0; i < 3; i++)
        {
            var p = result.Pages[i];
            Assert.Equal(i + 1, p.PageNumber);
            Assert.True(p.HasTextLayer);
            Assert.True(p.CharCount > 0);
            Assert.True(p.WordCount > 0);
            Assert.False(string.IsNullOrWhiteSpace(p.Text));
        }
    }

    [Fact]
    public void ObjStmCompressedPdf_ExtractsSuccessfully()
    {
        var path = Path.Combine(GetRepoRoot(), "test-fixtures", "objstm-compressed.pdf");
        Assert.True(File.Exists(path));

        var bytes = File.ReadAllBytes(path);
        var result = _service.Extract(bytes);

        Assert.True(result.Success);
        Assert.True(result.TotalPages > 0);
        Assert.NotEmpty(result.Pages);
    }

    [Fact]
    public void VietnameseDiacriticsPdf_ExtractsNormalizedFormCText()
    {
        var path = Path.Combine(GetRepoRoot(), "test-fixtures", "vietnamese-diacritics.pdf");
        Assert.True(File.Exists(path));

        var bytes = File.ReadAllBytes(path);
        var result = _service.Extract(bytes);

        Assert.True(result.Success);
        Assert.True(result.TotalPages > 0);

        foreach (var page in result.Pages)
        {
            Assert.True(page.Text.IsNormalized(NormalizationForm.FormC));
        }
    }

    [Fact]
    public void CorruptBrokenPdf_ReturnsCorruptFailure_NotSuccess()
    {
        var path = Path.Combine(GetRepoRoot(), "test-fixtures", "corrupt-broken.pdf");
        Assert.True(File.Exists(path));

        var bytes = File.ReadAllBytes(path);
        var result = _service.Extract(bytes);

        Assert.False(result.Success);
        Assert.True(result.IsCorrupt);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public void EncryptedPasswordPdf_ReturnsEncryptedFailure_NotSuccess()
    {
        var path = Path.Combine(GetRepoRoot(), "test-fixtures", "encrypted-password.pdf");
        Assert.True(File.Exists(path));

        var bytes = File.ReadAllBytes(path);
        var result = _service.Extract(bytes);

        Assert.False(result.Success);
        Assert.True(result.IsEncrypted);
        Assert.Contains("mật khẩu", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NonPdfBytes_ReturnsCorruptFailure()
    {
        var invalidBytes = Encoding.UTF8.GetBytes("This is definitely not a PDF file.");
        var result = _service.Extract(invalidBytes);

        Assert.False(result.Success);
        Assert.True(result.IsCorrupt);
    }

    [Fact]
    public void RealEbook_PhapLuatDaiCuong_Extracts298Pages()
    {
        var path = Path.Combine(GetRepoRoot(), "test", "Giáo trình Pháp luật đại cương_ebook.pdf");
        Assert.True(File.Exists(path));

        var result = _service.Extract(path);

        Assert.True(result.Success);
        Assert.Equal(298, result.TotalPages);
        Assert.Equal(298, result.Pages.Count);

        // Verify page 150 has genuine text layer and NFC normalized text
        var page150 = result.Pages[149];
        Assert.Equal(150, page150.PageNumber);
        Assert.True(page150.HasTextLayer);
        Assert.True(page150.CharCount > 0);
        Assert.True(page150.WordCount > 0);
        Assert.True(page150.Text.IsNormalized(NormalizationForm.FormC));
    }

    [Fact]
    public void RealEbook_NgonNguLapTrinhC_Extracts211Pages()
    {
        var path = Path.Combine(GetRepoRoot(), "test", "Ngôn ngữ lập trình C_ebook.pdf");
        Assert.True(File.Exists(path));

        var result = _service.Extract(path);

        Assert.True(result.Success);
        Assert.Equal(211, result.TotalPages);
        Assert.Equal(211, result.Pages.Count);
    }
}
