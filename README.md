# GodotFest — Visa Invitation Letter

A static Blazor WebAssembly tool that produces the Schengen visa invitation letter for a
**GodotFest 2026** attendee, in English and German, as Markdown and as a print-ready PDF.

Attendees travelling from visa-required countries need a formal invitation from the
organising company to submit with their Schengen application (Type C, business/conference).
This replaces hand-editing the Word template for every request.

> **Nothing leaves the browser.** There is no server and no API. Passport details live in
> memory for the length of the session and are never uploaded or stored. That is deliberate:
> the tool handles passport numbers and home addresses.

## What it produces

- **Markdown** — byte-for-byte compatible with the templates in the OneDrive workspace
  (`GodotFest - General/GodotFest 26/Visa/`).
- **PDF** — A4, real selectable and searchable text with proper tables and page numbers,
  ready to print, sign and stamp.

Files are named to match `Scripts\New-VisaInvitationLetter.ps1` in the OneDrive workspace,
so letters produced by either route sit together consistently:

```
Invitation-Letter-Ada-Lovelace-20260822.pdf      # English
Invitation-Letter-Ada-Lovelace-20260822-DE.pdf   # German
```

Choosing **Both** languages delivers a single `.zip`, because browsers block the second and
later downloads triggered by one click.

## Running it locally

```bash
dotnet run
```

Then open the URL it prints. Requires the .NET 10 SDK.

```bash
dotnet build      # compile
dotnet publish -c Release -o dist   # static output in dist/wwwroot
```

The published output is plain static files — any web server or object store will host it.

## What is fixed and what is typed in

Company and event facts are compiled into `Models/EventFacts.cs` and are **not** editable in
the UI. A typo in a register number on a document addressed to a consulate is a serious
problem, so those values are not left to a form field:

| Fixed | Value |
| --- | --- |
| Company | GameDev Events Munich UG (haftungsbeschränkt) |
| Address | Zschokkestr. 42, 80687 München, Germany |
| Register | Amtsgericht München, HRB 261121 |
| VAT ID | DE338446104 |
| Managing director | Senad Hrnjadovic |
| Event | GodotFest 2026, 3–4 November 2026 |
| Venue | smartvillage München-Bogenhausen, Rosenkavalierplatz 13, 81925 München |
| Stay | 2–5 November 2026 (4 days / 3 nights) |

Everything else — the attendee's identity, passport, employer, booking reference, and the
receiving embassy — is entered per letter.

## The cost clause

The letter states that the attendee bears all travel, accommodation, subsistence and
insurance costs, and that it is explicitly **not** a Verpflichtungserklärung under
§§ 66–68 AufenthG. That wording is what keeps liability off the UG. Do not soften it
without talking to Senad.

## Before sending a letter

1. Check the ticket actually exists in Pretix — every fact in the letter must hold.
2. Generate and print the PDF.
3. Senad signs as Geschäftsführer and applies the company stamp.
4. Scan at 300 dpi in colour.
5. Send with the enclosures:
   - Current Handelsregister extract (*Aktueller Ausdruck*, issued within 6 months)
   - Ticket/registration confirmation
   - Copy of the passport data page

The extract lives in the OneDrive workspace and must be under six months old on the date
of the application. See the README in `GodotFest 26/Visa/` for how to pull a fresh one.

The tool warns when a passport expires less than three months after the 5 November 2026
departure date, which is the Schengen minimum.

## How it is put together

One source of truth feeds both outputs, so the Markdown and the PDF can never drift apart:

```
LetterData ──► LetterComposer ──► LetterBlock[] ──┬──► MarkdownRenderer ──► .md
 (form input)   (EN / DE wording)  (structure)     └──► PdfDocumentFactory ──► pdfmake ──► .pdf
```

| Path | Purpose |
| --- | --- |
| `Models/LetterData.cs` | Form model and validation |
| `Models/EventFacts.cs` | Fixed company and event facts |
| `Letters/LetterComposer.cs` | The letter wording, English and German |
| `Letters/LetterBlock.cs` | Language-neutral document structure |
| `Letters/MarkdownRenderer.cs` | Blocks → Markdown |
| `Letters/PdfDocumentFactory.cs` | Blocks → pdfmake document definition |
| `Letters/LetterDates.cs` | Date formatting |
| `Services/ZipBuilder.cs` | Bundles both languages into one archive |
| `wwwroot/js/app.js` | pdfmake invocation and file saving |

### Why pdfmake

PDF generation runs client-side, which rules out a server-side renderer. The common
alternative — rasterising HTML with html2canvas — produces an image of the letter: not
selectable, not searchable, and poor quality when printed. pdfmake emits real text with
proper table layout, which is what a document submitted to a consulate needs.

pdfmake and its Roboto fonts are vendored under `wwwroot/lib/pdfmake/` rather than loaded
from a CDN, so the tool keeps working offline and has no third-party runtime dependency.
Roboto covers the German umlauts and the § and – characters the letter uses.

Month names are hardcoded in `LetterDates` instead of using `CultureInfo`, so the output is
identical regardless of which ICU data the WebAssembly runtime ships. This also lets the app
build with `InvariantGlobalization`.

## Deployment

`.github/workflows/deploy.yml` publishes the app to GitHub Pages. It builds on every push to
`main` and uploads the site as a build artifact; the deploy step only runs when the
repository variable `ENABLE_PAGES` is set to `true`.

To turn deployment on: enable Pages (Settings → Pages → Source: GitHub Actions), then add
the repository variable `ENABLE_PAGES=true`.

Note that **GitHub Pages on a private repository requires a paid plan** (Pro, Team or
Enterprise). On a free plan, either run the tool locally with `dotnet run` or host the
published `dist/wwwroot` folder somewhere else.

The workflow rewrites `<base href>` to the repository name, which is what a project Pages
site needs.
