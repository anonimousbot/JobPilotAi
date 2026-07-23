using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace JobPilotAi_Backend.Core.Files;

public sealed class LocalResumeStorage(
    IWebHostEnvironment environment,
    IOptions<FileUploadOptions> options) : IResumeStorage
{
    private readonly FileUploadOptions _options = options.Value;

    public async Task<ResumeStorageResult> SaveAsync(
        Guid userId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var uploadStream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await uploadStream.CopyToAsync(memory, cancellationToken);
        var bytes = memory.ToArray();
        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var relativePath = Path.Combine(_options.ResumeStoragePath, userId.ToString(), $"{hash}{extension}");
        var absolutePath = Path.Combine(environment.ContentRootPath, relativePath);
        var directory = Path.GetDirectoryName(absolutePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(absolutePath, bytes, cancellationToken);

        // Extract raw text based on file format
        var extractedText = extension switch
        {
            ".pdf" => ExtractTextFromPdf(bytes),
            ".docx" => ExtractTextFromDocx(bytes),
            _ => null
        };

        return new ResumeStorageResult(relativePath.Replace('\\', '/'), hash, extractedText);
    }

    private static string? ExtractTextFromPdf(byte[] bytes)
    {
        try
        {
            using var document = UglyToad.PdfPig.PdfDocument.Open(bytes);
            var sb = new System.Text.StringBuilder();
            foreach (var page in document.GetPages())
            {
                sb.AppendLine(page.Text);
            }
            return sb.ToString().Trim();
        }
        catch
        {
            return null;
        }
    }

    private static string? ExtractTextFromDocx(byte[] bytes)
    {
        try
        {
            using var memoryStream = new MemoryStream(bytes);
            using var archive = new System.IO.Compression.ZipArchive(memoryStream);
            var entry = archive.GetEntry("word/document.xml");
            if (entry is null) return null;

            using var reader = new StreamReader(entry.Open());
            var xml = reader.ReadToEnd();

            var sb = new System.Text.StringBuilder();
            var matches = System.Text.RegularExpressions.Regex.Matches(xml, @"<w:t[^>]*>(.*?)</w:t>");
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                sb.Append(match.Groups[1].Value).Append(' ');
            }
            return System.Net.WebUtility.HtmlDecode(sb.ToString().Trim());
        }
        catch
        {
            return null;
        }
    }
}
