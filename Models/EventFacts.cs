namespace GodotFest.VisaLetter.Models;

/// <summary>
/// Fixed company and event facts. These are deliberately not editable in the UI: they are
/// register facts and confirmed event details, and a typo in any of them on a document
/// addressed to a consulate is a serious problem.
/// </summary>
public static class EventFacts
{
    public const string CompanyName = "GameDev Events Munich UG (haftungsbeschränkt)";
    public const string Street = "Zschokkestr. 42";
    public const string PostcodeCity = "80687 München";
    public const string CountryEn = "Germany";
    public const string CountryDe = "Deutschland";
    public const string RegisterCourt = "Amtsgericht München";
    public const string RegisterNumber = "HRB 261121";
    public const string Register = $"{RegisterCourt}, {RegisterNumber}";
    public const string VatId = "DE338446104";
    public const string Director = "Senad Hrnjadovic";
    public const string Email = "contact@godotfest.com";
    public const string Website = "https://godotfest.com";

    public const string EventName = "GodotFest 2026";
    public const string VenueName = "smartvillage München-Bogenhausen";
    public const string VenueAddressEn = "Rosenkavalierplatz 13, 81925 München, Germany";
    public const string VenueAddressDe = "Rosenkavalierplatz 13, 81925 München, Deutschland";

    public static readonly DateOnly EventStart = new(2026, 11, 3);
    public static readonly DateOnly EventEnd = new(2026, 11, 4);
    public static readonly DateOnly Arrival = new(2026, 11, 2);
    public static readonly DateOnly Departure = new(2026, 11, 5);

    /// <summary>Departure plus the three months of validity Schengen rules require.</summary>
    public static readonly DateOnly PassportValidUntilAtLeast = Departure.AddMonths(3);
}
