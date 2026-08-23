using System.Text.Json.Serialization;

namespace GodotFest.VisaLetter.Models;

/// <summary>
/// What the attendee sends to the office: their data, not a finished document.
///
/// This is the whole point of the round trip. If the attendee emailed in a rendered PDF and
/// we simply stamped a signature onto it, we would be signing wording they controlled —
/// including the clause stating that the company bears no costs and that the letter is not a
/// Verpflichtungserklärung under §§ 66–68 AufenthG. Taking the data instead means the office
/// re-composes the letter with <c>LetterComposer</c>, so the wording is always ours.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class LetterRequest
{
    public const string KindValue = "godotfest.visa-letter-request";
    public const int CurrentSchemaVersion = 1;

    /// <summary>Identifies the file, so an unrelated .json is rejected with a clear message.</summary>
    public string Kind { get; set; } = KindValue;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    public DateTimeOffset Created { get; set; }

    public LetterData? Letter { get; set; }
}
