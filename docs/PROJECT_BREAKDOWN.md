# Project Breakdown

## 1. Extension Foundation

- [x] **SCL Syntax Highlighting** (TextMate)
  - [x] Configure `package.json` for `.scl` language.
  - [x] Define Basic Tokens (Keywords, Types, Operators).
  - [x] Create TextMate grammar (`syntaxes/scl.tmLanguage.json`).
- [ ] **SCL Snippets**
  - [ ] Add common control structures (IF, FOR, CASE, etc.).
- [ ] **LSP Client Setup**
  - [ ] Configure `LanguageClient` in `extension.ts`.
  - [ ] Establish process spawning for `PlatformPLC.Server`.
  - [ ] Implement Debug/Trace output channel.

## 2. Core Backend (`PlatformPLC.Core`)

- [ ] **Domain Models (AST)**
  - [ ] Define base classes for Program Organization Units (POUs).
  - [ ] Define Variable Types and Declarations.
  - [ ] Define Logic Structures (Expressions, Statements).
- [ ] **Parser Infrastructure**
  - [ ] Implement basic lexing/parsing for Generic SCL/ST.
  - [ ] Error handling and position tracking (Line/Column).
- [ ] **Interfaces**
  - [ ] Define `IPlatformAdapter` interface for vendor implementations.
  - [ ] Define `ILogicTranspiler` interface.

## 3. Language Server (`PlatformPLC.Server`)

- [ ] **LSP Protocol Handling**
  - [ ] Implement `Initialize` handshake.
  - [ ] Implement Text Document Synchronization (`textDocument/didOpen`, `didChange`, `didClose`).
- [ ] **Language Features**
  - [ ] **Diagnostics**: Publish parser errors to VS Code.
  - [ ] **Completion**: Basic keyword and variable autocomplete.
  - [ ] **Hover**: Type information and documentation on hover.
  - [ ] **Formatting**: Auto-format SCL code.

## 4. Adapters

- [ ] **Siemens Adapter** (`PlatformPLC.Adapters.Siemens`)
  - [ ] Implement `IPlatformAdapter`.
  - [ ] **Transpiler**: Generate Siemens-compatible SCL.
  - [ ] **Integration**: TIA Portal Openness / AML export (Optional).
- [ ] **Rockwell Adapter** (`PlatformPLC.Adapters.Rockwell`)
  - [ ] Implement `IPlatformAdapter`.
  - [ ] **Transpiler**: Generate Rockwell Structured Text / L5X XML.

## 5. Visual Editors (Webviews) -- _Phase 2_

- [ ] **Infrastructure**
  - [ ] Set up React/Vite build process for Webviews.
  - [ ] Implement Message Passing (Extension <-> Webview).
- [ ] **SFC Editor**
  - [ ] Visual Step/Transition rendering.
- [ ] **LAD Editor**
  - [ ] Component catalog and Drag & Drop.
