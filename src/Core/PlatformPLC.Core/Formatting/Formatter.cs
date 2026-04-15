namespace PlatformPLC.Core.Formatting;

/// <summary>
/// Base class for formatters. This class can be used to implement common functionality for all formatters/
/// Based in the IEC 61131-3 standard, a formatter is responsible for converting a value to a string representation and vice versa. 
/// </summary>
public abstract class Formatter : IFormatter
{
    public abstract string Format(string sourceText, FormattingOptions options);
}
