using GodotFest.VisaLetter.Letters;
using GodotFest.VisaLetter.Models;

namespace GodotFest.VisaLetter.Services;

/// <summary>
/// Turns letter data into downloads. Shared by the attendee page and the office signing page
/// so both go through the same <see cref="LetterComposer"/> call and cannot diverge.
/// </summary>
public sealed class LetterOutput(BrowserFiles files)
{
    public static LetterLanguage[] Languages(LetterOutputLanguage output) => output switch
    {
        LetterOutputLanguage.English => [LetterLanguage.English],
        LetterOutputLanguage.German => [LetterLanguage.German],
        _ => [LetterLanguage.English, LetterLanguage.German]
    };

    public async Task<string> DownloadMarkdownAsync(LetterData data, LetterIssue issue)
    {
        var documents = Languages(data.Output)
            .Select(language => (
                Name: $"{data.FileBaseName(language, issue.State)}.md",
                Text: MarkdownRenderer.Render(LetterComposer.Compose(data, language, issue))))
            .ToList();

        if (documents.Count == 1)
        {
            await files.DownloadMarkdownAsync(documents[0].Name, documents[0].Text);
            return "Markdown saved.";
        }

        var entries = documents
            .Select(d => (d.Name, Content: System.Text.Encoding.UTF8.GetBytes(d.Text)))
            .ToList();

        await files.DownloadZipAsync(
            $"{ArchiveBaseName(data, issue)}-markdown.zip", ZipBuilder.ToBase64(entries));
        return $"Markdown saved as a zip ({entries.Count} files).";
    }

    public async Task<string> DownloadPdfAsync(LetterData data, LetterIssue issue)
    {
        var languages = Languages(data.Output);

        if (languages.Length == 1)
        {
            var json = PdfDocumentFactory.BuildJson(
                LetterComposer.Compose(data, languages[0], issue), languages[0], issue);
            await files.DownloadPdfAsync($"{data.FileBaseName(languages[0], issue.State)}.pdf", json);
            return "PDF saved.";
        }

        var entries = new List<(string Name, byte[] Content)>();

        // Browsers block the second and later downloads triggered by one click, so several
        // languages have to arrive as a single archive.
        foreach (var language in languages)
        {
            var json = PdfDocumentFactory.BuildJson(
                LetterComposer.Compose(data, language, issue), language, issue);
            var base64 = await files.PdfBase64Async(json);
            entries.Add(($"{data.FileBaseName(language, issue.State)}.pdf", Convert.FromBase64String(base64)));
        }

        await files.DownloadZipAsync($"{ArchiveBaseName(data, issue)}-pdf.zip", ZipBuilder.ToBase64(entries));
        return $"PDF saved as a zip ({entries.Count} files).";
    }

    public async Task OpenPdfAsync(LetterData data, LetterLanguage language, LetterIssue issue)
    {
        var json = PdfDocumentFactory.BuildJson(
            LetterComposer.Compose(data, language, issue), language, issue);
        await files.OpenPdfAsync(json);
    }

    private static string ArchiveBaseName(LetterData data, LetterIssue issue) =>
        data.FileBaseName(LetterLanguage.English, issue.State);
}
