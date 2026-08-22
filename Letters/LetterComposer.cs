using GodotFest.VisaLetter.Models;

namespace GodotFest.VisaLetter.Letters;

/// <summary>
/// Turns the form data into the letter. The wording is a faithful port of the templates in
/// the OneDrive workspace (GodotFest 26\Visa\), including the clause stating that the company
/// assumes no costs and that the letter is not a Verpflichtungserklärung under §§ 66–68
/// AufenthG. Do not soften that wording — it is what keeps liability off the UG.
/// </summary>
public static class LetterComposer
{
    public static IReadOnlyList<LetterBlock> Compose(LetterData data, LetterLanguage language) =>
        language == LetterLanguage.German ? ComposeGerman(data) : ComposeEnglish(data);

    /// <summary>Empty fields fall back to the same {{PLACEHOLDER}} markers the Word templates use.</summary>
    private static string F(string value, string placeholder) =>
        string.IsNullOrWhiteSpace(value) ? $"{{{{{placeholder}}}}}" : value.Trim();

    /// <summary>Collapses a multi-line entry onto one line for use inside a table cell.</summary>
    private static string Flat(string value, string placeholder)
    {
        var text = F(value, placeholder);
        return string.Join(", ", text
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static IReadOnlyList<string> SplitLines(string value, string placeholder)
    {
        var text = F(value, placeholder);
        var lines = text
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        return lines.Count > 0 ? lines : [text];
    }

    private static string EventDays(LetterLanguage language)
    {
        var start = EventFacts.EventStart;
        var end = EventFacts.EventEnd;
        var startDay = LetterDates.Weekday(start, language);
        var endDay = LetterDates.Weekday(end, language);
        var monthYear = LetterDates.Format(end, language);

        return language == LetterLanguage.German
            ? $"{startDay}, {start.Day}. und {endDay}, {monthYear}"
            : $"{startDay} {start.Day} and {endDay} {monthYear}";
    }

    private static List<LetterBlock> ComposeEnglish(LetterData d)
    {
        const LetterLanguage lang = LetterLanguage.English;

        var name = F(d.FullName, "FULL_NAME");
        var employer = F(d.Employer, "EMPLOYER");
        var ticket = F(d.TicketOrderId, "TICKET_ORDER_ID");
        var eventDays = EventDays(lang);
        var arrival = LetterDates.Format(EventFacts.Arrival, lang);
        var departure = LetterDates.Format(EventFacts.Departure, lang);
        var eventStart = LetterDates.Format(EventFacts.EventStart, lang);
        var eventEnd = LetterDates.Format(EventFacts.EventEnd, lang);
        var conferenceDays = $"{EventFacts.EventStart.Day} and {eventEnd}";

        return
        [
            new HeadingBlock(1, EventFacts.CompanyName),
            new LinesBlock(
            [
                $"{EventFacts.Street} · {EventFacts.PostcodeCity} · {EventFacts.CountryEn}",
                $"Handelsregister: {EventFacts.Register} · VAT ID: {EventFacts.VatId}",
                $"Geschäftsführer: {EventFacts.Director}",
                $"{EventFacts.Email} · {EventFacts.Website}"
            ]),
            new RuleBlock(),
            new LinesBlock(
            [
                F(d.MissionName, "MISSION_NAME"),
                "Visa Section",
                .. SplitLines(d.MissionAddress, "MISSION_ADDRESS")
            ]),
            new LinesBlock([$"Munich, {LetterDates.Format(d.LetterDate, lang, "{{LETTER_DATE}}")}"]),
            new HeadingBlock(2, "Letter of Invitation — Schengen Visa (Type C), Business / Conference"),
            new ParagraphBlock(
                $"**Re: Invitation of {name} to attend {EventFacts.EventName} in Munich, Germany, " +
                $"from {EventFacts.EventStart.Day} to {eventEnd}**"),
            new ParagraphBlock("Dear Sir or Madam,"),
            new ParagraphBlock(
                $"we, {EventFacts.CompanyName}, hereby confirm that we have invited the person named " +
                $"below to attend **{EventFacts.EventName}**, a professional business conference for the " +
                "Godot game engine, which we are organising in Munich, Germany."),
            new ParagraphBlock(
                "We kindly ask you to support this application with the issuance of a Schengen visa for " +
                "the period stated in this letter."),

            new HeadingBlock(3, "1. Inviting company"),
            new TableBlock("Field", "Details",
            [
                new TableRow("Company name", EventFacts.CompanyName),
                new TableRow("Street address", EventFacts.Street),
                new TableRow("Postal code and city", EventFacts.PostcodeCity),
                new TableRow("Country", EventFacts.CountryEn),
                new TableRow("Commercial register", EventFacts.Register),
                new TableRow("VAT identification number", EventFacts.VatId),
                new TableRow("Legal representative", $"{EventFacts.Director}, Geschäftsführer (Managing Director)"),
                new TableRow("Contact email", EventFacts.Email),
                new TableRow("Website", EventFacts.Website)
            ]),
            new ParagraphBlock(
                "A current extract from the German Commercial Register (Handelsregister), issued no more " +
                "than six months before the date of this letter, is enclosed with this invitation."),

            new HeadingBlock(3, "2. Person invited"),
            new TableBlock("Field", "Details",
            [
                new TableRow("Full name (as in passport)", name),
                new TableRow("Date of birth", LetterDates.Format(d.DateOfBirth, lang, "{{DATE_OF_BIRTH}}")),
                new TableRow("Place and country of birth", Flat(d.PlaceOfBirth, "PLACE_OF_BIRTH")),
                new TableRow("Nationality", F(d.Nationality, "NATIONALITY")),
                new TableRow("Passport number", F(d.PassportNumber, "PASSPORT_NUMBER")),
                new TableRow("Passport issued on", LetterDates.Format(d.PassportIssueDate, lang, "{{PASSPORT_ISSUE_DATE}}")),
                new TableRow("Passport valid until", LetterDates.Format(d.PassportExpiryDate, lang, "{{PASSPORT_EXPIRY_DATE}}")),
                new TableRow("Residential address", Flat(d.HomeAddress, "HOME_ADDRESS")),
                new TableRow("Employer / company", employer),
                new TableRow("Position", F(d.JobTitle, "JOB_TITLE"))
            ]),

            new HeadingBlock(3, "3. Purpose and objective of the stay"),
            new ParagraphBlock(
                $"The sole purpose of the stay is to attend {EventFacts.EventName} as a **{d.RoleEnglish}**."),
            new ParagraphBlock(
                "GodotFest is the European business conference for the Godot game engine. The programme " +
                "consists of a keynote main stage and a side stage with 30 to 40 professional talks on game " +
                "development, engine technology, tooling, and studio practice, together with a structured " +
                "business networking programme operated through the MeetToMatch platform, a sponsor and " +
                "studio showcase, and dedicated meeting spaces for scheduled one-to-one industry meetings."),
            new ParagraphBlock(
                "The objectives of the visit are professional development, technical knowledge exchange with " +
                "other studios and engine contributors, and the initiation and continuation of business " +
                "relationships within the international game development industry. The visit is strictly of a " +
                "business and professional nature. It does not serve tourism, and no employment of any kind " +
                "will be taken up in Germany."),
            new ParagraphBlock(
                $"The registration of {name} for {EventFacts.EventName} is confirmed under booking " +
                $"reference **{ticket}**."),

            new HeadingBlock(3, "4. Company to be visited and event details"),
            new TableBlock("Field", "Details",
            [
                new TableRow("Company to be visited", EventFacts.CompanyName),
                new TableRow("Event", EventFacts.EventName),
                new TableRow("Event dates", eventDays),
                new TableRow("Venue", EventFacts.VenueName),
                new TableRow("Venue address", EventFacts.VenueAddressEn),
                new TableRow("Daily programme", "approximately 09:00 to 18:00 on both days")
            ]),

            new HeadingBlock(3, "5. Period and duration of the stay"),
            new TableBlock("Field", "Details",
            [
                new TableRow("Intended date of arrival in Germany", arrival),
                new TableRow("Intended date of departure from Germany", departure),
                new TableRow("Total duration of stay", "4 days / 3 nights"),
                new TableRow("Place of stay", "Munich, Germany"),
                new TableRow("Accommodation", Flat(d.AccommodationEn, "ACCOMMODATION"))
            ]),
            new ParagraphBlock(
                $"The stay covers the two conference days on {conferenceDays}, preceded by one day for " +
                $"arrival and travel on {arrival} and followed by one day for departure on {departure}. " +
                "No stay beyond this period is intended, and the entire stay takes place within Munich, Germany."),

            new HeadingBlock(3, "6. Costs and financial responsibility"),
            new ParagraphBlock(
                "All costs arising from this visit — including international travel, local transport, " +
                "accommodation, subsistence, and travel health insurance — are borne in full by " +
                $"{name} and/or their employer, {employer}."),
            new ParagraphBlock(
                $"{EventFacts.CompanyName} does **not** assume any costs in connection with this visit. " +
                "For the avoidance of doubt, this letter is an invitation for the purpose of attending our " +
                "conference only. It does **not** constitute a formal declaration of commitment " +
                "(Verpflichtungserklärung) pursuant to Sections 66 to 68 of the German Residence Act " +
                "(§§ 66–68 AufenthG), and no liability for public funds is accepted by the undersigned or " +
                "by the company."),

            new HeadingBlock(3, "7. Confirmations"),
            new ParagraphBlock("We confirm to the best of our knowledge that:"),
            new BulletsBlock(
            [
                $"The invitation is issued solely for attendance at {EventFacts.EventName} on {conferenceDays}.",
                $"{name} will not take up any employment or paid activity in Germany or in any other Schengen state during the stay.",
                $"{name} is expected to leave the Schengen area on {departure} and in any case before the expiry of the visa granted.",
                "The invitee is responsible for holding valid travel health insurance for the full duration of the stay."
            ]),
            new ParagraphBlock(
                "Should you require any further information or documentation in support of this application, " +
                $"please contact us at {EventFacts.Email}. We are glad to answer any questions your office may have."),
            new ParagraphBlock("Yours faithfully,"),
            new GapBlock(),
            new GapBlock(),
            new SignatureLineBlock(),
            new LinesBlock(
            [
                $"**{EventFacts.Director}**",
                "Geschäftsführer (Managing Director)",
                EventFacts.CompanyName
            ]),
            new NoteBlock("(signature and company stamp)"),
            new RuleBlock(),
            new HeadingBlock(3, "Enclosures"),
            new OrderedBlock(
            [
                "Current extract from the German Commercial Register (Aktueller Ausdruck, Handelsregister), issued no more than 6 months ago",
                $"Confirmation of conference registration, booking reference {ticket}",
                $"Copy of the passport data page of {name}"
            ])
        ];
    }

    private static List<LetterBlock> ComposeGerman(LetterData d)
    {
        const LetterLanguage lang = LetterLanguage.German;

        var name = F(d.FullName, "FULL_NAME");
        var employer = F(d.Employer, "EMPLOYER");
        var ticket = F(d.TicketOrderId, "TICKET_ORDER_ID");
        var eventDays = EventDays(lang);
        var arrival = LetterDates.Format(EventFacts.Arrival, lang);
        var departure = LetterDates.Format(EventFacts.Departure, lang);
        var eventEnd = LetterDates.Format(EventFacts.EventEnd, lang);
        var conferenceDays = $"{EventFacts.EventStart.Day}. und {eventEnd}";

        return
        [
            new HeadingBlock(1, EventFacts.CompanyName),
            new LinesBlock(
            [
                $"{EventFacts.Street} · {EventFacts.PostcodeCity} · {EventFacts.CountryDe}",
                $"Handelsregister: {EventFacts.Register} · USt-IdNr.: {EventFacts.VatId}",
                $"Geschäftsführer: {EventFacts.Director}",
                $"{EventFacts.Email} · {EventFacts.Website}"
            ]),
            new RuleBlock(),
            new LinesBlock(
            [
                F(d.MissionName, "MISSION_NAME"),
                "Visastelle",
                .. SplitLines(d.MissionAddress, "MISSION_ADDRESS")
            ]),
            new LinesBlock([$"München, {LetterDates.Format(d.LetterDate, lang, "{{LETTER_DATE}}")}"]),
            new HeadingBlock(2, "Einladungsschreiben — Schengen-Visum (Typ C), Geschäftsreise / Konferenz"),
            new ParagraphBlock(
                $"**Betreff: Einladung von {name} zur Teilnahme an der {EventFacts.EventName} in München, " +
                $"Deutschland, vom {EventFacts.EventStart.Day}. bis {eventEnd}**"),
            new ParagraphBlock("Sehr geehrte Damen und Herren,"),
            new ParagraphBlock(
                $"hiermit bestätigen wir, die {EventFacts.CompanyName}, dass wir die nachfolgend genannte " +
                $"Person zur Teilnahme an der **{EventFacts.EventName}** eingeladen haben, einer Fachkonferenz " +
                "für die Godot Game Engine, die wir in München, Deutschland, veranstalten."),
            new ParagraphBlock(
                "Wir bitten Sie höflich, den Antrag durch die Erteilung eines Schengen-Visums für den in " +
                "diesem Schreiben genannten Zeitraum zu unterstützen."),

            new HeadingBlock(3, "1. Einladendes Unternehmen"),
            new TableBlock("Feld", "Angaben",
            [
                new TableRow("Firma", EventFacts.CompanyName),
                new TableRow("Straße", EventFacts.Street),
                new TableRow("PLZ und Ort", EventFacts.PostcodeCity),
                new TableRow("Land", EventFacts.CountryDe),
                new TableRow("Handelsregister", EventFacts.Register),
                new TableRow("Umsatzsteuer-Identifikationsnummer", EventFacts.VatId),
                new TableRow("Gesetzlicher Vertreter", $"{EventFacts.Director}, Geschäftsführer"),
                new TableRow("E-Mail", EventFacts.Email),
                new TableRow("Website", EventFacts.Website)
            ]),
            new ParagraphBlock(
                "Ein aktueller Auszug aus dem Handelsregister, dessen Abruf nicht länger als sechs Monate vor " +
                "dem Datum dieses Schreibens zurückliegt, ist diesem Schreiben beigefügt."),

            new HeadingBlock(3, "2. Eingeladene Person"),
            new TableBlock("Feld", "Angaben",
            [
                new TableRow("Vollständiger Name (wie im Reisepass)", name),
                new TableRow("Geburtsdatum", LetterDates.Format(d.DateOfBirth, lang, "{{DATE_OF_BIRTH}}")),
                new TableRow("Geburtsort und -land", Flat(d.PlaceOfBirth, "PLACE_OF_BIRTH")),
                new TableRow("Staatsangehörigkeit", F(d.Nationality, "NATIONALITY")),
                new TableRow("Reisepassnummer", F(d.PassportNumber, "PASSPORT_NUMBER")),
                new TableRow("Reisepass ausgestellt am", LetterDates.Format(d.PassportIssueDate, lang, "{{PASSPORT_ISSUE_DATE}}")),
                new TableRow("Reisepass gültig bis", LetterDates.Format(d.PassportExpiryDate, lang, "{{PASSPORT_EXPIRY_DATE}}")),
                new TableRow("Wohnanschrift", Flat(d.HomeAddress, "HOME_ADDRESS")),
                new TableRow("Arbeitgeber", employer),
                new TableRow("Position", F(d.JobTitle, "JOB_TITLE"))
            ]),

            new HeadingBlock(3, "3. Zweck, Ziel und Art des Aufenthalts"),
            new ParagraphBlock(
                $"Der Aufenthalt dient ausschließlich der Teilnahme an der {EventFacts.EventName} als **{d.RoleGerman}**."),
            new ParagraphBlock(
                "GodotFest ist die europäische Fachkonferenz für die Godot Game Engine. Das Programm umfasst " +
                "eine Hauptbühne und eine Nebenbühne mit 30 bis 40 Fachvorträgen zu Spieleentwicklung, " +
                "Engine-Technologie, Entwicklungswerkzeugen und Studiopraxis, ein strukturiertes " +
                "Business-Networking-Programm über die Plattform MeetToMatch, eine Sponsoren- und " +
                "Studio-Ausstellung sowie gesonderte Besprechungsräume für terminierte Einzelgespräche."),
            new ParagraphBlock(
                "Ziele des Besuchs sind die fachliche Weiterbildung, der technische Wissensaustausch mit " +
                "anderen Studios und Engine-Entwicklern sowie die Anbahnung und Fortführung von " +
                "Geschäftsbeziehungen innerhalb der internationalen Spieleentwicklungsbranche. Der Besuch ist " +
                "ausschließlich geschäftlicher und beruflicher Natur. Er dient nicht touristischen Zwecken, " +
                "und es wird in Deutschland keinerlei Erwerbstätigkeit aufgenommen."),
            new ParagraphBlock(
                $"Die Anmeldung von {name} zur {EventFacts.EventName} ist unter der Buchungsreferenz " +
                $"**{ticket}** bestätigt."),

            new HeadingBlock(3, "4. Besuchtes Unternehmen und Veranstaltungsdaten"),
            new TableBlock("Feld", "Angaben",
            [
                new TableRow("Besuchtes Unternehmen", EventFacts.CompanyName),
                new TableRow("Veranstaltung", EventFacts.EventName),
                new TableRow("Veranstaltungstage", eventDays),
                new TableRow("Veranstaltungsort", EventFacts.VenueName),
                new TableRow("Anschrift des Veranstaltungsortes", EventFacts.VenueAddressDe),
                new TableRow("Tagesprogramm", "jeweils ca. 09:00 bis 18:00 Uhr")
            ]),

            new HeadingBlock(3, "5. Zeitraum und Dauer des Aufenthalts"),
            new TableBlock("Feld", "Angaben",
            [
                new TableRow("Geplante Einreise nach Deutschland", arrival),
                new TableRow("Geplante Ausreise aus Deutschland", departure),
                new TableRow("Gesamtdauer des Aufenthalts", "4 Tage / 3 Nächte"),
                new TableRow("Aufenthaltsort", "München, Deutschland"),
                new TableRow("Unterkunft", Flat(d.AccommodationDe, "ACCOMMODATION_DE"))
            ]),
            new ParagraphBlock(
                $"Der Aufenthalt umfasst die beiden Konferenztage am {conferenceDays} sowie einen Anreisetag " +
                $"am {arrival} und einen Abreisetag am {departure}. Ein darüber hinausgehender Aufenthalt ist " +
                "nicht beabsichtigt; der gesamte Aufenthalt findet in München, Deutschland, statt."),

            new HeadingBlock(3, "6. Kosten und Kostenträgerschaft"),
            new ParagraphBlock(
                "Sämtliche im Zusammenhang mit diesem Besuch entstehenden Kosten — einschließlich " +
                "internationaler Reisekosten, Kosten des örtlichen Transports, der Unterkunft, des " +
                "Lebensunterhalts sowie der Reisekrankenversicherung — werden vollständig von " +
                $"{name} und/oder dem Arbeitgeber, {employer}, getragen."),
            new ParagraphBlock(
                $"Die {EventFacts.CompanyName} übernimmt **keine** Kosten im Zusammenhang mit diesem Besuch. " +
                "Klarstellend weisen wir darauf hin, dass es sich bei diesem Schreiben ausschließlich um eine " +
                "Einladung zur Teilnahme an unserer Konferenz handelt. Es stellt **keine** " +
                "Verpflichtungserklärung nach §§ 66 bis 68 des Aufenthaltsgesetzes (AufenthG) dar; eine " +
                "Haftung für öffentliche Mittel wird weder vom Unterzeichner noch von der Gesellschaft " +
                "übernommen."),

            new HeadingBlock(3, "7. Bestätigungen"),
            new ParagraphBlock("Wir bestätigen nach bestem Wissen:"),
            new BulletsBlock(
            [
                $"Die Einladung wird ausschließlich zum Zweck der Teilnahme an der {EventFacts.EventName} am {conferenceDays} ausgesprochen.",
                $"{name} wird während des Aufenthalts weder in Deutschland noch in einem anderen Schengen-Staat eine Erwerbstätigkeit oder sonstige entgeltliche Tätigkeit aufnehmen.",
                $"{name} wird das Schengen-Gebiet voraussichtlich am {departure}, in jedem Fall jedoch vor Ablauf des erteilten Visums, wieder verlassen.",
                "Die eingeladene Person ist selbst dafür verantwortlich, für die gesamte Dauer des Aufenthalts eine gültige Reisekrankenversicherung vorzuhalten."
            ]),
            new ParagraphBlock(
                $"Für Rückfragen oder weitere Unterlagen stehen wir Ihnen unter {EventFacts.Email} gerne zur Verfügung."),
            new ParagraphBlock("Mit freundlichen Grüßen"),
            new GapBlock(),
            new GapBlock(),
            new SignatureLineBlock(),
            new LinesBlock(
            [
                $"**{EventFacts.Director}**",
                "Geschäftsführer",
                EventFacts.CompanyName
            ]),
            new NoteBlock("(Unterschrift und Firmenstempel)"),
            new RuleBlock(),
            new HeadingBlock(3, "Anlagen"),
            new OrderedBlock(
            [
                "Aktueller Ausdruck aus dem Handelsregister, Abruf nicht älter als 6 Monate",
                $"Anmeldebestätigung zur Konferenz, Buchungsreferenz {ticket}",
                $"Kopie der Passdatenseite von {name}"
            ])
        ];
    }
}
