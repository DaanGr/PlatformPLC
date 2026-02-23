// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import path from "path";
import * as vscode from "vscode";

import {
  CloseAction,
  ErrorAction,
  LanguageClient,
  LanguageClientOptions,
  ServerOptions,
  TransportKind,
} from "vscode-languageclient/node";

let client: LanguageClient;
// This method is called when your extension is activated
// Your extension is activated the very first time the command is executed
export function activate(context: vscode.ExtensionContext) {
  const serverModule = context.asAbsolutePath(
    path.join("server", "PlatformPLC.Server.dll"),
  );

  let serverOptions: ServerOptions = {
    command: "dotnet",
    args: [serverModule],
    transport: TransportKind.stdio,
  };

  let clientOptions: LanguageClientOptions = {
    documentSelector: [{ scheme: "file", language: "scl" }],
    outputChannel: vscode.window.createOutputChannel("PLC Language Server"),
    errorHandler: {
      error: (error, message, count) => {
        console.error("Language Server Error:", error);
        return { action: ErrorAction.Continue }; // Continue
      },
      closed: () => {
        console.warn("Language Server closed unexpectedly.");
        return { action: CloseAction.Restart }; // Restart
      },
    },
  };

  client = new LanguageClient(
    "plcLanguageServer",
    "PLC Language Server",
    serverOptions,
    clientOptions,
  );

  client.start();
  context.subscriptions.push(client);
}

// This method is called when your extension is deactivated
export function deactivate() {
  if (!client) {
    return undefined;
  }

  return client.stop();
}
