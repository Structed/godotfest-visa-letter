using Microsoft.JSInterop;

namespace GodotFest.VisaLetter.Services;

/// <summary>Thin wrapper over wwwroot/js/app.js.</summary>
public sealed class BrowserFiles(IJSRuntime js)
{
    public ValueTask DownloadMarkdownAsync(string fileName, string markdown) =>
        js.InvokeVoidAsync("visaLetter.downloadText", fileName, markdown);

    public ValueTask DownloadPdfAsync(string fileName, string docDefinitionJson) =>
        js.InvokeVoidAsync("visaLetter.downloadPdf", fileName, docDefinitionJson);

    public ValueTask<string> PdfBase64Async(string docDefinitionJson) =>
        js.InvokeAsync<string>("visaLetter.pdfBase64", docDefinitionJson);

    public ValueTask DownloadZipAsync(string fileName, string base64) =>
        js.InvokeVoidAsync("visaLetter.downloadBase64", fileName, base64, "application/zip");

    public ValueTask OpenPdfAsync(string docDefinitionJson) =>
        js.InvokeVoidAsync("visaLetter.openPdf", docDefinitionJson);

    public ValueTask<bool> CopyAsync(string text) =>
        js.InvokeAsync<bool>("visaLetter.copyText", text);
}
