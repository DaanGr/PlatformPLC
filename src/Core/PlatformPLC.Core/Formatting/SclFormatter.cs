using System.Text.RegularExpressions;

namespace PlatformPLC.Core.Formatting;

public partial class SclFormatter : Formatter
{
    // Ordered longest-first so compound keywords (e.g. FUNCTION_BLOCK) are matched before shorter ones (e.g. FUNCTION).
    private static readonly string[] s_keywords =
    [
        "FUNCTION_BLOCK", "END_FUNCTION_BLOCK",
        "ORGANIZATION_BLOCK", "END_ORGANIZATION_BLOCK",
        "END_FUNCTION", "FUNCTION",
        "VAR_INPUT", "VAR_OUTPUT", "VAR_IN_OUT", "VAR_TEMP", "VAR_STAT",
        "END_VAR", "VAR",
        "END_STRUCT", "STRUCT",
        "END_REGION", "REGION",
        "END_REPEAT", "REPEAT",
        "END_WHILE", "WHILE",
        "END_FOR", "FOR",
        "END_CASE", "CASE",
        "END_IF", "IF",
        "END_TYPE", "TYPE",
        "END_PROGRAM", "PROGRAM",
        "END_CONST", "CONST",
        "ELSIF", "ELSE", "THEN", "UNTIL",
        "TO", "BY", "DO", "OF",
        "RETURN", "CONTINUE", "EXIT",
        "AND", "OR", "NOT", "XOR", "MOD",
        "TRUE", "FALSE",
        "RETAIN", "PERSISTENT", "CONSTANT", "AT",
        "R_EDGE", "F_EDGE",
        "ARRAY",
        "BOOL", "BYTE", "WORD", "DWORD", "LWORD",
        "SINT", "USINT", "INT", "UINT", "DINT", "UDINT", "LINT", "ULINT",
        "REAL", "LREAL",
        "CHAR", "WCHAR", "STRING", "WSTRING",
        "TIME", "DATE", "TOD", "DT", "LTIME", "LTOD", "LDT",
    ];

    // Pre-compiled case-insensitive regexes for each keyword, built from s_keywords above.
    private static readonly Regex[] s_keywordRegexes = Array.ConvertAll(
        s_keywords,
        kw => new Regex($@"\b{Regex.Escape(kw)}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled));

    /// <summary>
    /// Keywords that reduce the indent level <em>before</em> their line is emitted.
    /// Note: UNTIL is treated as the body-closer for REPEAT blocks, so END_REPEAT is
    /// intentionally absent here — it stays at the same level as REPEAT.
    /// </summary>
    private static readonly HashSet<string> s_closers = new(StringComparer.Ordinal)
    {
        "END_IF", "END_FOR", "END_WHILE",
        "END_FUNCTION_BLOCK", "END_FUNCTION",
        "END_ORGANIZATION_BLOCK", "END_PROGRAM",
        "END_REGION", "END_VAR", "END_STRUCT",
        "END_TYPE", "END_CONST",
        "UNTIL",
    };

    /// <summary>
    /// Keywords that reduce the indent level before their line <em>and</em> re-indent after it
    /// (i.e. they are at the same level as the block-opener).
    /// </summary>
    private static readonly HashSet<string> s_continuators = new(StringComparer.Ordinal)
    {
        "ELSE", "ELSIF",
    };

    /// <summary>Keywords that open a new block when they <em>start</em> a line.</summary>
    private static readonly HashSet<string> s_blockStartOpeners = new(StringComparer.Ordinal)
    {
        "REPEAT",
        "FUNCTION_BLOCK", "FUNCTION", "ORGANIZATION_BLOCK", "PROGRAM",
        "STRUCT", "TYPE", "CONST",
        "VAR", "VAR_INPUT", "VAR_OUTPUT", "VAR_IN_OUT", "VAR_TEMP", "VAR_STAT",
        "REGION",
    };

    /// <summary>Keywords that open a new block when they <em>end</em> a line.</summary>
    private static readonly HashSet<string> s_lineEndOpeners = new(StringComparer.Ordinal)
    {
        "THEN", "DO",
        // OF is intentionally absent: CASE...OF opens two levels (label tier + body tier).
    };

    public override string Format(string sourceText, FormattingOptions options)
    {
        string lineEnding = DetectLineEnding(sourceText);
        string[] lines = sourceText.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
        List<string> result = new(lines.Length);
        int indentLevel = 0;
        int consecutiveBlanks = 0;
        bool inBlockComment = false;
        int blockCommentIndentLevel = 0;

        foreach (string rawLine in lines)
        {
            string trimmed = rawLine.Trim();

            if (trimmed.Length == 0)
            {
                if (++consecutiveBlanks <= 1)
                {
                    result.Add(string.Empty);
                }

                continue;
            }

            consecutiveBlanks = 0;

            // Lines inside a block comment are emitted verbatim; the closing *) gets
            // the same indent level as the line that opened the block comment.
            if (inBlockComment)
            {
                if (trimmed.Contains("*)"))
                {
                    inBlockComment = false;
                    result.Add(GetIndent(blockCommentIndentLevel, options) + trimmed);
                }
                else
                {
                    result.Add(rawLine);
                }

                continue;
            }

            // Detect a block comment that opens on this line but does not close on it.
            if (trimmed.Contains("(*") && !trimmed.Contains("*)"))
            {
                inBlockComment = true;
                blockCommentIndentLevel = indentLevel;
            }

            (string codePart, string commentPart) = SplitAtLineComment(trimmed);

            codePart = UppercaseKeywords(codePart);
            codePart = NormalizeOperators(codePart);
            codePart = NormalizeParentheses(codePart);

            string processedLine = commentPart.Length > 0
                ? codePart.TrimEnd() + " " + commentPart
                : codePart.TrimEnd();

            string leadingKeyword = GetLeadingKeyword(processedLine);
            bool isCaseLabel = IsCaseLabel(codePart);

            // Dedent before closers, continuators, and CASE labels.
            // END_CASE closes two levels (label tier + body tier).
            if (leadingKeyword == "END_CASE")
            {
                indentLevel = Math.Max(0, indentLevel - 2);
            }
            else if (s_closers.Contains(leadingKeyword) || s_continuators.Contains(leadingKeyword) || isCaseLabel)
            {
                indentLevel = Math.Max(0, indentLevel - 1);
            }

            result.Add(GetIndent(indentLevel, options) + processedLine);

            // Re-indent after continuators, CASE labels, and block openers.
            // CASE...OF opens two levels so labels are indented under CASE and body under labels.
            if (GetTrailingKeyword(codePart) == "OF")
            {
                indentLevel += 2;
            }
            else if (s_continuators.Contains(leadingKeyword) || isCaseLabel || IsLineOpener(codePart))
            {
                indentLevel++;
            }
        }

        return string.Join(lineEnding, result);
    }

    private static bool IsLineOpener(string codeLine)
    {
        // A line opens a block if it ends with THEN / DO / OF ...
        if (s_lineEndOpeners.Contains(GetTrailingKeyword(codeLine)))
        {
            return true;
        }

        // ... or if it starts a block-level declaration.
        return s_blockStartOpeners.Contains(GetLeadingKeyword(codeLine));
    }

    /// <summary>
    /// Detects a CASE branch label such as <c>10:</c>, <c>1..5:</c>, or <c>myLabel:</c>.
    /// ELSE/ELSIF lines that end with <c>:</c> are already handled as continuators and excluded.
    /// </summary>
    private static bool IsCaseLabel(string codePart)
    {
        string code = codePart.TrimEnd().TrimEnd(';').TrimEnd();
        if (!code.EndsWith(':'))
        {
            return false;
        }

        string leading = GetLeadingKeyword(code);
        return !s_closers.Contains(leading)
            && !s_continuators.Contains(leading)
            && !s_blockStartOpeners.Contains(leading)
            && !s_lineEndOpeners.Contains(leading);
    }

    private static string GetLeadingKeyword(string line)
    {
        Match match = LeadingKeywordRegex().Match(line);
        return match.Success ? match.Groups[1].Value.ToUpperInvariant() : string.Empty;
    }

    private static string GetTrailingKeyword(string line)
    {
        string stripped = line.TrimEnd().TrimEnd(';').TrimEnd();
        Match match = TrailingKeywordRegex().Match(stripped);
        return match.Success ? match.Groups[1].Value.ToUpperInvariant() : string.Empty;
    }

    private static string UppercaseKeywords(string code)
    {
        for (int i = 0; i < s_keywordRegexes.Length; i++)
        {
            code = s_keywordRegexes[i].Replace(code, s_keywords[i]);
        }

        return code;
    }

    private static string NormalizeOperators(string code)
    {
        // Normalize assignment and comparison operators to have exactly one space on each side.
        // Order matters: multi-char operators must be replaced before their single-char prefixes.
        code = AssignmentOperatorRegex().Replace(code, " := ");
        code = NotEqualOperatorRegex().Replace(code, " <> ");
        code = LessEqualOperatorRegex().Replace(code, " <= ");
        code = GreaterEqualOperatorRegex().Replace(code, " >= ");

        // Remove any leading space that normalization may have introduced on the first token.
        return code.TrimStart();
    }

    /// <summary>
    /// Removes whitespace between consecutive opening or closing parentheses
    /// so that e.g. <c>( ( (x) ) )</c> becomes <c>(((x)))</c>.
    /// </summary>
    private static string NormalizeParentheses(string code)
    {
        // Loop until stable so that triple (or deeper) nesting is also collapsed.
        string previous;
        do
        {
            previous = code;
            code = ConsecutiveOpenParenRegex().Replace(code, "((");
            code = ConsecutiveCloseParenRegex().Replace(code, "))");
        }
        while (!ReferenceEquals(code, previous) && code != previous);

        return code;
    }

    /// <summary>
    /// Splits a trimmed line into the SCL code portion and the trailing <c>//</c> comment
    /// (if any), correctly ignoring <c>/</c> characters inside string literals.
    /// </summary>
    private static (string code, string comment) SplitAtLineComment(string line)
    {
        bool inString = false;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '\'')
            {
                inString = !inString;
            }
            else if (!inString && i + 1 < line.Length && line[i] == '/' && line[i + 1] == '/')
            {
                return (line[..i].TrimEnd(), line[i..]);
            }
        }

        return (line, string.Empty);
    }

    private static string DetectLineEnding(string text)
    {
        return text.Contains("\r\n") ? "\r\n" : text.Contains('\r') ? "\r" : "\n";
    }

    private static string GetIndent(int level, FormattingOptions options)
    {
        return options.UseTabs
            ? new string('\t', level)
            : new string(' ', level * options.TabSize);
    }

    [GeneratedRegex(@"^([A-Za-z_][A-Za-z0-9_]*)")]
    private static partial Regex LeadingKeywordRegex();

    [GeneratedRegex(@"([A-Za-z_][A-Za-z0-9_]*)$")]
    private static partial Regex TrailingKeywordRegex();

    [GeneratedRegex(@"\s*:=\s*")]
    private static partial Regex AssignmentOperatorRegex();

    [GeneratedRegex(@"\s*<>\s*")]
    private static partial Regex NotEqualOperatorRegex();

    [GeneratedRegex(@"\s*<=\s*")]
    private static partial Regex LessEqualOperatorRegex();

    [GeneratedRegex(@"\s*>=\s*")]
    private static partial Regex GreaterEqualOperatorRegex();

    [GeneratedRegex(@"\(\s+\(")]
    private static partial Regex ConsecutiveOpenParenRegex();

    [GeneratedRegex(@"\)\s+\)")]
    private static partial Regex ConsecutiveCloseParenRegex();
}