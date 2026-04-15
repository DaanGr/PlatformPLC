
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using PlatformPLC.Server;

// Ensure stdout is not written to by anything other than the JSON-RPC transport.
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

LanguageServer server = await LanguageServer.From(options =>
    options
        .WithInput(Console.OpenStandardInput())
        .WithOutput(Console.OpenStandardOutput())
        .ConfigureLogging(logging =>
            logging
                .AddLanguageProtocolLogging()
                .SetMinimumLevel(LogLevel.Warning))
        .WithHandler<DocumentFormattingHandler>()
).ConfigureAwait(true);

await server.WaitForExit.ConfigureAwait(true);
