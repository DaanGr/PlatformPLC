# Implementation Plan: PlatformPLC

This guide outlines the step-by-step process to build PlatformPLC from scratch, following the `DESIGN_OVERVIEW.md` architecture.

## Phase 1: Repository Scaffolding (Day 1-2)

**Goal:** Establish the Monorepo structure and ensure the Frontend can launch the Backend.

1.  **Initialize Folders**:
    - Create `src/Core`, `src/Extension`, and `src/Adapters`.
2.  **Setup .NET Solution (`src/PlatformPLC.sln`)**:
    - Create `PlatformPLC.Core` (Class Lib).
    - Create `PlatformPLC.Server` (Console App).
    - Create `PlatformPLC.Adapters.Siemens/Rockwell` (Class Libs).
3.  **Setup VS Code Extension**:
    - Run `yo code` inside `src/Extension` to enforce TypeScript + Webpack.
    - Configure `launch.json` to allow "F5" debugging of both the Extension and the .NET Server simultaneously.
4.  **Establish Communications**:
    - Implement standard Input/Output redirection in `PlatformPLC.Server` (read from Stdin, write to Stdout).
    - Use `vscode-languageclient` in the extension to spawn the `.exe` and verify a "Hello World" handshake.

## Phase 2: The Language Server Protocol (LSP) (Week 1-2)

**Goal:** Get basic IntelliSense (Diagnostics) working for a dummy file.

1.  **LSP Implementation**:
    - Install `OmniSharp.Extensions.LanguageServer` in `PlatformPLC.Server`.
    - Implement `ITextDocumentSyncHandler` to track file open/change/save events.
2.  **Basic Parsing**:
    - Create a simple grammar (ANTLR4) for structured text in `PlatformPLC.Core`.
    - On `TextDocumentDidChange`, parse the text and publish `Diagnostics` (errors/warnings) back to VS Code.

## Phase 3: Data Modeling & Transpiler (Week 3-4)

**Goal:** Define the internal data model and converting it to text.

1.  **Define Models**:
    - Create C# classes for `PlcBlock`, `Tag`, `Udt`, `Step`, `Transition`.
2.  **Transpiler Logic**:
    - Implement `IVendorAdapter` interface.
    - **Siemens**: Implement text generation for `.scl` files.
    - **Rockwell**: Implement XML generation for `.L5X` segments.

## Phase 4: Visual Editors (Webviews) (Week 5-6)

**Goal:** Allow graphical editing of logic.

1.  **React Setup**:
    - Create a standalone React+Vite app in `src/Extension/webviews`.
2.  **VS Code Integration**:
    - Implement a `CustomTextEditorProvider`.
    - Load the React build into the Webview.
    - Sync changes: `Webview -> JSON -> Extension -> File System`.

## Phase 5: Vendor Integration (Week 7+)

**Goal:** Connect to the outside world.

1.  **Siemens Openness**:
    - Import `Siemens.Engineering.dll` in the Adapter project.
    - Implement Sync command: `Model -> TIA Portal`.
2.  **Rockwell Import/Export**:
    - Finalize `.L5X` wrapping for full project import in Studio 5000.

## Checklist for Success

- [ ] **Keep Core Pure**: Never reference Vendor DLLs in `PlatformPLC.Core`.
- [ ] **Test Driven**: Write unit tests for the Transpiler before the UI exists.
- [ ] **Dogfooding**: Try to write a simple traffic light program using your own tool early on.
