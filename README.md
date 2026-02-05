# PlatformPLC

PlatformPLC is a universal, open-source framework designed to bring PLC engineering into the modern software development era. It allows engineers to design hardware-agnostic logic within **VS Code**, leveraging professional software engineering tools such as Git and Language Servers.

---

## Vision

Industrial automation has long been confined to heavy, proprietary vendor software. PlatformPLC acts as an **Abstraction Layer** above industrial standards. Write your code once in a modern IDE and deploy it to various hardware platforms, including **Siemens S7** and **Allen Bradley Logix**.

---

## Architecture

The system is built on a three-tier model to ensure modularity and scalability:

1. **The Core (C# .NET):** The central intelligence of the platform. It hosts the **Language Server Protocol (LSP)**, the **Code Formatter**, and the **Sync Engine**.
2. **Vendor Adapters:** Modular drivers that translate neutral logic into hardware-specific formats:
* **Siemens Adapter:** Interfaces with TIA Portal Openness API for block injection and project management.
* **Rockwell Adapter:** Utilizes the Logix Designer SDK and manipulates L5X (XML) files for Studio 5000 integration.


3. **VS Code Extension:** A lightweight frontend that manages the connection to the Core and hosts visual editors (SFC/LAD) via Webviews.

---

## Core Features

### 1. Universal Language Server (LSP)

PlatformPLC provides full IDE intelligence for SCL (Siemens) and ST (Allen Bradley):

* **Auto-complete:** Suggestions based on live or cached Tag tables and UDT structures.
* **Go-to-Definition:** Instant navigation from a block call to its source definition.
* **Real-time Diagnostics:** Syntax validation powered by an ANTLR4-based C# parser.

### 2. Visual Logic Designers (SFC & LAD)

Design graphical logic directly within VS Code:

* **SFC Editor:** A visual tool for designing sequences that are transpiled into optimized SCL/ST `CASE` structures.
* **LAD Editor:** A graphical interface for Ladder Diagramming, utilizing the C# Core for network validation and XML generation.

### 3. Git-Centric Workflow

Move away from comparing binary project files. PlatformPLC exports every block, tag table, and UDT as a plain text file.

* Perform granular code reviews with `git diff`.
* Enable multi-user collaboration without merge conflicts in proprietary databases.

---

## Installation

### Via Winget

PlatformPLC is distributed via the Windows Package Manager for easy installation and updates:

```powershell
winget install PlatformPLC

```

### VS Code Extension

Search for "PlatformPLC" in the VS Code Marketplace or install it via the command line:

```powershell
code --install-extension platformplc.extension

```

---
