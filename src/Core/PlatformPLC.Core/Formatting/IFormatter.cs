namespace PlatformPLC.Core.Formatting;

public interface IFormatter
{
    string Format(string sourceText, FormattingOptions options);
}

