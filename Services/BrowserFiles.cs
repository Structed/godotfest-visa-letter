using Microsoft.JSInterop;

namespace GodotFest.VisaLetter.Services;

/// <summary>Thin wrapper over wwwroot/js/app.js.</summary>
public sealed class BrowserFiles(IJSRuntime js)
{
    public ValueTask DownloadMarkdownAsync(string fileName, string markdown) =>
        js.InvokeVoidAsync("visaLetter.downloadText", fileName, markdown, "text/markdown;charset=utf-8");

    public ValueTask DownloadJsonAsync(string fileName, string json) =>
        js.InvokeVoidAsync("visaLetter.downloadText", fileName, json, "application/json;charset=utf-8");

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

    public ValueTask<bool> RememberSignatureAsync(string dataUrl) =>
        js.InvokeAsync<bool>("visaLetter.rememberSignature", dataUrl);

    public ValueTask<string?> RecallSignatureAsync() =>
        js.InvokeAsync<string?>("visaLetter.recallSignature");

    public ValueTask ForgetSignatureAsync() =>
        js.InvokeVoidAsync("visaLetter.forgetSignature");
}
