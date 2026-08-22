# GodotFest — Visa Invitation Letter

A static Blazor WebAssembly tool that produces the Schengen visa invitation letter for a
**GodotFest 2026** attendee, in English and German, as Markdown and as a print-ready PDF.

**Live tool: <https://structed.github.io/godotfest-visa-letter/>**

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

### In the GitHub Copilot app

`.github/github-app.yml` configures the project for the GitHub Copilot app. It restores
packages when a session is created, and offers **Run**, **Build** and **Publish** scripts.
**Run** serves the app on <http://localhost:5279> and the app opens it in its integrated
browser.

That script passes `--no-launch-profile`, so `Properties/launchSettings.json` doesn't open a
second browser window on top of it; `--urls` keeps the port the same as the launch profile.
The hosting environment is still `Development`, because the WebAssembly dev server defaults
to it.

The app will ask you to review and accept the configuration before running anything from it,
and will ask again each time the file changes.

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

`.github/workflows/deploy.yml` publishes the app to GitHub Pages on every push to `main`,
which includes every merged pull request. The live site is:

<https://structed.github.io/godotfest-visa-letter/>

Pull requests targeting `main` run the same build without deploying, so a publish that would
break the site is caught before it merges. Pages is configured with **Source: GitHub
Actions** (Settings → Pages); nothing gates the deployment behind a repository variable.

Three things have to be done to the published output for a project Pages site to work, and
the workflow does all three:

| Step | Why |
| --- | --- |
| Rewrite `<base href>` to `/godotfest-visa-letter/` | The site is served from a subpath, not the domain root. `wwwroot/index.html` keeps `<base href="/" />` so `dotnet run` still works locally. |
| Write `.nojekyll` | Otherwise Jekyll strips `_framework/`, and the app cannot load its own runtime. |
| Copy `index.html` to `404.html` | Lets deep links resolve to the SPA. |

The base href rewrite is verified after it runs, because `sed` exits successfully when it
matches nothing — an unnoticed miss would deploy a site that requests its framework files
from the wrong path and never boots.

Publishing the site changes nothing about how the tool handles data. The deployed output is
the same static files, there is still no backend, and passport details still never leave the
browser. A publicly reachable URL is not a data-handling concern here.
