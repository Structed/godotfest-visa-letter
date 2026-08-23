namespace GodotFest.VisaLetter.Letters;

/// <summary>
/// A language-neutral description of the letter. Both the Markdown writer and the PDF
/// builder consume this, so the two outputs can never drift apart.
/// Inline emphasis is expressed with Markdown-style ** ** markers and translated by
/// each renderer.
/// </summary>
public abstract record LetterBlock;

/// <summary>Tightly grouped lines such as the letterhead or the address block.</summary>
public sealed record LinesBlock(IReadOnlyList<string> Lines) : LetterBlock;

public sealed record HeadingBlock(int Level, string Text) : LetterBlock;

public sealed record ParagraphBlock(string Text) : LetterBlock;

public sealed record TableRow(string Label, string Value);

public sealed record TableBlock(string HeaderLabel, string HeaderValue, IReadOnlyList<TableRow> Rows) : LetterBlock;

public sealed record BulletsBlock(IReadOnlyList<string> Items) : LetterBlock;

public sealed record OrderedBlock(IReadOnlyList<string> Items) : LetterBlock;

public sealed record RuleBlock : LetterBlock;

/// <summary>Vertical whitespace left for a handwritten signature.</summary>
public sealed record GapBlock : LetterBlock;

public sealed record SignatureLineBlock : LetterBlock;

/// <summary>
/// An embedded signature image, used in place of the blank signature line when the letter is
/// issued signed. <paramref name="Caption"/> is what the Markdown writer emits instead: a
/// multi-megabyte base64 data URL inline in a .md file would be useless to a reader.
/// </summary>
public sealed record SignatureImageBlock(string PngDataUrl, double WidthPt, string Caption) : LetterBlock;

/// <summary>
/// Marks an unsigned review copy, so a draft the attendee generated for themselves cannot be
/// mistaken for — or submitted as — an issued letter.
/// </summary>
public sealed record DraftMarkBlock(string Label, string Note) : LetterBlock;

public sealed record NoteBlock(string Text) : LetterBlock;
