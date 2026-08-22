using System.Text.Json;
using GodotFest.VisaLetter.Models;

namespace GodotFest.VisaLetter.Letters;

/// <summary>
/// Builds a pdfmake document definition from the same blocks the Markdown writer uses.
/// pdfmake was chosen over an HTML-to-canvas approach because it emits real, selectable,
/// searchable text with proper table layout — a rasterised screenshot is not acceptable
/// for a document submitted to a consulate.
/// </summary>
public static class PdfDocumentFactory
{
    private const double ContentWidth = 483; // A4 (595.28pt) minus 56pt margins each side.

    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = false,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string BuildJson(IReadOnlyList<LetterBlock> blocks, LetterLanguage language)
    {
        var content = new List<object>();

        foreach (var block in blocks)
        {
            switch (block)
            {
                case HeadingBlock { Level: 1 } h:
                    content.Add(Node(h.Text, "companyName"));
                    break;

                case HeadingBlock { Level: 2 } h:
                    content.Add(Node(h.Text, "h2"));
                    break;

                case HeadingBlock h:
                    content.Add(Node(h.Text, "h3"));
                    break;

                case LinesBlock l:
                    content.Add(new Dictionary<string, object>
                    {
                        ["text"] = l.Lines.SelectMany((line, i) =>
                        {
                            var runs = Inline(line);
                            if (i < l.Lines.Count - 1)
                            {
                                runs.Add(Run("\n", false));
                            }

                            return runs;
                        }).ToList(),
                        ["style"] = "lines"
                    });
                    break;

                case ParagraphBlock p:
                    content.Add(new Dictionary<string, object>
                    {
                        ["text"] = Inline(p.Text),
                        ["style"] = "paragraph"
                    });
                    break;

                case TableBlock t:
                    content.Add(TableNode(t));
                    break;

                case BulletsBlock b:
                    content.Add(new Dictionary<string, object>
                    {
                        ["ul"] = b.Items.Select(i => new Dictionary<string, object> { ["text"] = Inline(i) }).ToList(),
                        ["style"] = "list"
                    });
                    break;

                case OrderedBlock o:
                    content.Add(new Dictionary<string, object>
                    {
                        ["ol"] = o.Items.Select(i => new Dictionary<string, object> { ["text"] = Inline(i) }).ToList(),
                        ["style"] = "list"
                    });
                    break;

                case RuleBlock:
                    content.Add(new Dictionary<string, object>
                    {
                        ["canvas"] = new List<object>
                        {
                            new Dictionary<string, object>
                            {
                                ["type"] = "line",
                                ["x1"] = 0,
                                ["y1"] = 0,
                                ["x2"] = ContentWidth,
                                ["y2"] = 0,
                                ["lineWidth"] = 0.6,
                                ["lineColor"] = "#c8ced6"
                            }
                        },
                        ["margin"] = new[] { 0, 6, 0, 12 }
                    });
                    break;

                case GapBlock:
                    content.Add(new Dictionary<string, object>
                    {
                        ["text"] = " ",
                        ["margin"] = new[] { 0, 0, 0, 16 }
                    });
                    break;

                case SignatureLineBlock:
                    content.Add(new Dictionary<string, object>
                    {
                        ["text"] = "_______________________________________",
                        ["margin"] = new[] { 0, 0, 0, 3 }
                    });
                    break;

                case NoteBlock n:
                    content.Add(Node(n.Text, "note"));
                    break;
            }
        }

        var doc = new Dictionary<string, object>
        {
            ["pageSize"] = "A4",
            ["pageMargins"] = new[] { 56, 54, 56, 60 },
            ["info"] = new Dictionary<string, object>
            {
                ["title"] = language == LetterLanguage.German
                    ? "Einladungsschreiben — Schengen-Visum"
                    : "Letter of Invitation — Schengen Visa",
                ["author"] = EventFacts.CompanyName
            },
            ["defaultStyle"] = new Dictionary<string, object>
            {
                ["font"] = "Roboto",
                ["fontSize"] = 9.5,
                ["lineHeight"] = 1.3,
                ["color"] = "#14181d"
            },
            // Consumed by the JS layer, which turns it into pdfmake's footer callback.
            ["footerLabel"] = language == LetterLanguage.German ? "Seite" : "Page",
            ["footerOf"] = language == LetterLanguage.German ? "von" : "of",
            ["content"] = content,
            ["styles"] = Styles()
        };

        return JsonSerializer.Serialize(doc, Json);
    }

    private static Dictionary<string, object> Styles() => new()
    {
        ["companyName"] = new Dictionary<string, object>
        {
            ["fontSize"] = 15,
            ["bold"] = true,
            ["color"] = "#2b5f8c",
            ["margin"] = new[] { 0, 0, 0, 5 }
        },
        ["lines"] = new Dictionary<string, object>
        {
            ["fontSize"] = 9,
            ["lineHeight"] = 1.25,
            ["margin"] = new[] { 0, 0, 0, 10 }
        },
        ["h2"] = new Dictionary<string, object>
        {
            ["fontSize"] = 12.5,
            ["bold"] = true,
            ["margin"] = new[] { 0, 10, 0, 8 }
        },
        ["h3"] = new Dictionary<string, object>
        {
            ["fontSize"] = 10.5,
            ["bold"] = true,
            ["color"] = "#2b5f8c",
            ["margin"] = new[] { 0, 12, 0, 5 }
        },
        ["paragraph"] = new Dictionary<string, object>
        {
            ["alignment"] = "justify",
            ["margin"] = new[] { 0, 0, 0, 7 }
        },
        ["list"] = new Dictionary<string, object>
        {
            ["margin"] = new[] { 0, 2, 0, 9 }
        },
        ["note"] = new Dictionary<string, object>
        {
            ["italics"] = true,
            ["color"] = "#6b7480",
            ["fontSize"] = 8.5,
            ["margin"] = new[] { 0, 6, 0, 10 }
        },
        ["tableHeader"] = new Dictionary<string, object>
        {
            ["bold"] = true,
            ["fontSize"] = 9,
            ["color"] = "#2b5f8c"
        }
    };

    private static Dictionary<string, object> TableNode(TableBlock t)
    {
        var body = new List<object>
        {
            new List<object>
            {
                new Dictionary<string, object> { ["text"] = t.HeaderLabel, ["style"] = "tableHeader" },
                new Dictionary<string, object> { ["text"] = t.HeaderValue, ["style"] = "tableHeader" }
            }
        };

        foreach (var row in t.Rows)
        {
            body.Add(new List<object>
            {
                new Dictionary<string, object> { ["text"] = Inline(row.Label), ["color"] = "#3d454f" },
                new Dictionary<string, object> { ["text"] = Inline(row.Value) }
            });
        }

        return new Dictionary<string, object>
        {
            ["table"] = new Dictionary<string, object>
            {
                ["headerRows"] = 1,
                ["widths"] = new object[] { "38%", "62%" },
                ["body"] = body
            },
            ["layout"] = "godotfest",
            ["fontSize"] = 9,
            ["margin"] = new[] { 0, 2, 0, 10 }
        };
    }

    private static Dictionary<string, object> Node(string text, string style) => new()
    {
        ["text"] = text,
        ["style"] = style
    };

    private static Dictionary<string, object> Run(string text, bool bold) => new()
    {
        ["text"] = text,
        ["bold"] = bold
    };

    /// <summary>Splits Markdown-style **bold** spans into pdfmake text runs.</summary>
    private static List<object> Inline(string text)
    {
        var runs = new List<object>();
        var i = 0;

        while (i < text.Length)
        {
            var start = text.IndexOf("**", i, StringComparison.Ordinal);
            if (start < 0)
            {
                runs.Add(Run(text[i..], false));
                break;
            }

            var end = text.IndexOf("**", start + 2, StringComparison.Ordinal);
            if (end < 0)
            {
                runs.Add(Run(text[i..], false));
                break;
            }

            if (start > i)
            {
                runs.Add(Run(text[i..start], false));
            }

            runs.Add(Run(text[(start + 2)..end], true));
            i = end + 2;
        }

        if (runs.Count == 0)
        {
            runs.Add(Run(text, false));
        }

        return runs;
    }
}
