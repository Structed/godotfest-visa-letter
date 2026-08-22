using GodotFest.VisaLetter.Models;

namespace GodotFest.VisaLetter.Letters;

/// <summary>
/// Month names are hardcoded rather than taken from <see cref="System.Globalization"/> so
/// the output is identical everywhere regardless of which ICU data the WebAssembly runtime
/// happens to ship. The letter must read the same for every operator.
/// </summary>
public static class LetterDates
{
    private static readonly string[] EnglishMonths =
    [
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];

    private static readonly string[] GermanMonths =
    [
        "Januar", "Februar", "März", "April", "Mai", "Juni",
        "Juli", "August", "September", "Oktober", "November", "Dezember"
    ];

    private static readonly string[] EnglishWeekdays =
    [
        "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
    ];

    private static readonly string[] GermanWeekdays =
    [
        "Sonntag", "Montag", "Dienstag", "Mittwoch", "Donnerstag", "Freitag", "Samstag"
    ];

    public static string Format(DateOnly date, LetterLanguage language) => language == LetterLanguage.German
        ? $"{date.Day}. {GermanMonths[date.Month - 1]} {date.Year}"
        : $"{date.Day} {EnglishMonths[date.Month - 1]} {date.Year}";

    public static string Format(DateOnly? date, LetterLanguage language, string placeholder) =>
        date is { } value ? Format(value, language) : placeholder;

    public static string Weekday(DateOnly date, LetterLanguage language) => language == LetterLanguage.German
        ? GermanWeekdays[(int)date.DayOfWeek]
        : EnglishWeekdays[(int)date.DayOfWeek];
}
