using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using PlatformPLC.Core.Formatting;

namespace PlatformPLC.Server;

internal sealed class DocumentFormattingHandler : DocumentFormattingHandlerBase
{
    protected override DocumentFormattingRegistrationOptions CreateRegistrationOptions(
        DocumentFormattingCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new DocumentFormattingRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("scl"),
        };
    }

    public override Task<TextEditContainer?> Handle(
        DocumentFormattingParams request,
        CancellationToken cancellationToken)
    {
        string filePath = DocumentUri.GetFileSystemPath(request.TextDocument.Uri)
            ?? throw new InvalidDataException("Could not resolve file path from URI.");

        string sourceText = File.ReadAllText(filePath);

        Core.Formatting.FormattingOptions formattingOptions = new()
        {
            TabSize = request.Options.TabSize,
            UseTabs = !request.Options.InsertSpaces,
        };

        string formatted = new SclFormatter().Format(sourceText, formattingOptions);

        // Return a single edit replacing the entire document.
        TextEdit edit = new()
        {
            Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(new Position(0, 0), new Position(int.MaxValue, int.MaxValue)),
            NewText = formatted,
        };

        return Task.FromResult<TextEditContainer?>(new TextEditContainer(edit));
    }
}
