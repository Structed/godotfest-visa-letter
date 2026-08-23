using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using GodotFest.VisaLetter.Models;

namespace GodotFest.VisaLetter.Services;

/// <summary>The outcome of reading a request file the office received by email.</summary>
public sealed record LetterRequestParse(
    bool Ok,
    LetterData? Data,
    string Message,
    IReadOnlyList<string> Problems)
{
    public static LetterRequestParse Failed(string message, IReadOnlyList<string>? problems = null) =>
        new(false, null, message, problems ?? []);
}

/// <summary>
/// Reads and writes the request file the attendee emails to the office. Written indented and
/// in plain English keys on purpose: the file carries a passport number and a home address,
/// so the attendee should be able to open it and see exactly what they are sending.
/// </summary>
public static class LetterRequestFile
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string Write(LetterData data) =>
        JsonSerializer.Serialize(
            new LetterRequest { Created = DateTimeOffset.UtcNow, Letter = data },
            Options);

    public static LetterRequestParse Read(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return LetterRequestParse.Failed("There is nothing to read — paste the request file or choose it from disk.");
        }

        LetterRequest? request;

        try
        {
            // Unmapped members are rejected rather than ignored, so a file that has been
            // hand-edited into a shape we do not recognise fails loudly instead of quietly
            // dropping whatever was added. Someone adding a "companyName" key hoping it will
            // override a register fact should hear about it, not be silently ignored.
            request = JsonSerializer.Deserialize<LetterRequest>(json, Options);
        }
        catch (JsonException ex)
        {
            return LetterRequestParse.Failed(
                "That request file could not be read. If it has been edited by hand, ask the attendee " +
                "to generate a fresh one rather than repairing it.",
                [Explain(ex)]);
        }

        if (request is null || request.Letter is null)
        {
            return LetterRequestParse.Failed("That file does not contain any letter data.");
        }

        if (!string.Equals(request.Kind, LetterRequest.KindValue, StringComparison.Ordinal))
        {
            return LetterRequestParse.Failed(
                $"That file is not a GodotFest visa letter request (kind was \"{request.Kind}\").");
        }

        if (request.SchemaVersion != LetterRequest.CurrentSchemaVersion)
        {
            return LetterRequestParse.Failed(
                $"That request file is version {request.SchemaVersion}, but this tool reads version " +
                $"{LetterRequest.CurrentSchemaVersion}. Ask the attendee to generate a fresh one.");
        }

        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(
            request.Letter, new ValidationContext(request.Letter), results, validateAllProperties: true);

        if (!valid)
        {
            return new LetterRequestParse(
                false,
                request.Letter,
                "The request is incomplete — ask the attendee to fill in the missing details and send it again.",
                results.Select(r => r.ErrorMessage ?? "Invalid value.").ToList());
        }

        return new LetterRequestParse(true, request.Letter, "Request loaded. Check the details before signing.", []);
    }

    /// <summary>
    /// Turns a serialiser message into something an operator can act on. The unmapped-member
    /// case is the one that matters: it means the file carries a field this tool does not
    /// recognise, which is exactly what a hand-edited file looks like.
    /// </summary>
    private static string Explain(JsonException ex)
    {
        const string unmapped = "UnmappedJsonProperty";
        var message = ex.Message;

        if (!message.Contains(unmapped, StringComparison.Ordinal))
        {
            return string.IsNullOrWhiteSpace(ex.Path)
                ? message
                : $"{message} (at {ex.Path})";
        }

        var field = message
            .Split(',', StringSplitOptions.TrimEntries)
            .Skip(1)
            .FirstOrDefault();

        return string.IsNullOrEmpty(field)
            ? "The file contains a field this tool does not recognise."
            : $"The file contains a field this tool does not recognise: \"{field}\".";
    }
}
