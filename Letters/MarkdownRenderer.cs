using System.Text;

namespace GodotFest.VisaLetter.Letters;

/// <summary>
/// Writes the letter blocks back out as Markdown, byte-for-byte compatible with the
/// templates stored in the OneDrive workspace so both routes produce the same document.
/// </summary>
public static class MarkdownRenderer
{
    public static string Render(IReadOnlyList<LetterBlock> blocks)
    {
        var sb = new StringBuilder();

        foreach (var block in blocks)
        {
            switch (block)
            {
                case HeadingBlock h:
                    sb.Append('#', h.Level).Append(' ').AppendLine(h.Text);
                    sb.AppendLine();
                    break;

                case LinesBlock l:
                    foreach (var line in l.Lines)
                    {
                        sb.AppendLine(line);
                    }

                    sb.AppendLine();
                    break;

                case ParagraphBlock p:
                    sb.AppendLine(p.Text);
                    sb.AppendLine();
                    break;

                case TableBlock t:
                    sb.AppendLine($"| {t.HeaderLabel} | {t.HeaderValue} |");
                    sb.AppendLine("| --- | --- |");
                    foreach (var row in t.Rows)
                    {
                        sb.AppendLine($"| {row.Label} | {row.Value} |");
                    }

                    sb.AppendLine();
                    break;

                case BulletsBlock b:
                    foreach (var item in b.Items)
                    {
                        sb.AppendLine($"- {item}");
                    }

                    sb.AppendLine();
                    break;

                case OrderedBlock o:
                    for (var i = 0; i < o.Items.Count; i++)
                    {
                        sb.AppendLine($"{i + 1}. {o.Items[i]}");
                    }

                    sb.AppendLine();
                    break;

                case RuleBlock:
                    sb.AppendLine("---");
                    sb.AppendLine();
                    break;

                case GapBlock:
                    sb.AppendLine("&nbsp;");
                    sb.AppendLine();
                    break;

                // Deliberately no trailing blank line: the signature name block sits
                // directly underneath the rule in the source templates.
                case SignatureLineBlock:
                    sb.AppendLine("_______________________________________");
                    break;

                case NoteBlock n:
                    sb.AppendLine($"*{n.Text}*");
                    sb.AppendLine();
                    break;
            }
        }

        return sb.ToString().TrimEnd() + "\n";
    }
}
