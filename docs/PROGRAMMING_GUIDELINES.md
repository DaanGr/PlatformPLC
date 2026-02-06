# Programming Guidelines: PlatformPLC

This document establishes the coding standards for **PlatformPLC**. These rules are designed to minimize technical debt and automate administrative tasks for a solo developer.

---

## 1. General Principles

- **Clean Architecture**: The `Core` project contains domain logic and interfaces only. It must have **zero** dependencies on `Adapters`, `Infrastructure`, or external frameworks (e.g., VS Code API, Roslyn).
- **Enforced Style**:
- **C#**: Use a `Directory.Build.props` file to enforce `<Nullable>enable</Nullable>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
- **TypeScript**: Use ESLint + Prettier.
- **Tooling**: The **C# Dev Kit**, **ESLint**, and **Prettier** extensions must be used.

- **Commits**: Strictly follow [Conventional Commits](https://www.conventionalcommits.org/). This is the **foundation** for our automated versioning.

---

## 2. Workflow & Versioning

### Branching Strategy (Trunk-Based)

To maintain velocity without sacrificing stability, we use a single-trunk model with mandatory "self-reviews."

- **Main Branch (`main`)**: The source of truth. Every commit must pass CI (build + tests).
- **Feature Branches**: Use `type/description` (e.g., `feat/sfc-parser`).
- **The PR Gate**: Even as a solo dev, all changes enter `main` via a **Pull Request**.
- **Rule**: Use **Squash and Merge**. This collapses messy dev history into one clean, semantic commit on `main`.

### Versioning (SemVer)

We decouple **Development Bumps** from **Release Tags** to prevent version inflation.

- **Pre-Release (v0.x.y)**: While in initial development, we stay in `0.x.y`. Breaking changes increment `0.x`, features/fixes increment `0.x.y`.
- **Magnitude**:
- `feat:` -> Minor bump.
- `fix:` -> Patch bump.
- `BREAKING CHANGE:` -> Major bump.

- **The Release Trigger**: Versions are calculated and files (`package.json`, `AssemblyInfo.cs`) are updated **only** when a GitHub Release is manually triggered.
- **Tooling**: Use `GitVersion` or `semantic-release` to automate the calculation based on squashed commits.

---

## 3. C# Guidelines (.NET Backend)

### Naming & Style

- **Standard**: PascalCase for Classes/Methods/Properties; camelCase for locals/parameters.
- **Interfaces**: Prefix with `I` (`IParser`).
- **Async**: Suffix all Task-returning methods with `Async`.

### Error Handling & Safety

- **Result Pattern**: Avoid throwing exceptions for expected failures (e.g., "PLC not found"). Return a `Result<T>` object.
- **DI**: Use Constructor Injection exclusively. Avoid `static` for logic to ensure the LSP remains thread-safe.

---

## 4. TypeScript Guidelines (VS Code Extension)

### Naming & Style

- **Files**: `kebab-case.ts`.
- **Interfaces**: PascalCase, **no** `I` prefix.
- **Strictness**: `no-explicit-any` is enabled. Use `unknown` and type guards for external data (like JSON payloads).

### Resource Management

- **Disposables**: Every event listener, command, or provider **must** be pushed to `context.subscriptions`.
- **Communication**: Use a shared `Messages.ts` to define discriminated unions for Webview-to-Extension communication.

---

## 5. Testing & Quality Gate

- **Core Logic**: 100% coverage for Parsers/Transpilers via **xUnit**.
- **LSP**: Use a mock LSP client to test completion and diagnostics without launching a full VS Code instance.
- **Local Enforcement**:
- **Husky**: Pre-commit hook runs `dotnet format` and `npm run lint`.
- **CI**: GitHub Actions runs all tests on every PR.

---
