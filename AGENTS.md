# AGENTS.md — LibLR1

## Purpose

LibLR1 is the **core data and format library** for the Lego Racers modding ecosystem.

It is the authoritative source for:

- binary file format definitions
- parsing logic
- shared data structures (e.g., SKB_Gradient)
- serialization/deserialization behavior

All other tools (Track Editor, Binary Editor, Online Mod, etc.) depend on LibLR1.

---

## Project Structure & Module Organization

`LibLR1.sln` contains two C# projects targeting .NET Framework 4.8.

### LibLR1/
Core library containing parsers and data structures.

- Most top-level `*.cs` files are parsers named after file types:
  - `ADB.cs`, `GDB.cs`, `SKB.cs`, etc.
- Shared helpers:
  - `IO/`
  - `Utils/`
  - `Exceptions/`

### Tester/
Console-based validation harness for running parsers against real game assets.
Location of the locally extracted LEGO.JAM files is located in localGameFiles.json.

---

## Core Architecture Rules

### 1. LibLR1 is the Source of Truth

- All shared file formats must be defined here
- Do NOT duplicate format logic in other projects
- Do NOT allow other tools to define their own versions of structures

If another project needs a structure:
➡️ define or update it here first

---

### 2. Architecture Constraints

LibLR1 must remain:

- engine-agnostic
- UI-agnostic
- tool-agnostic

Do NOT introduce:

- MonoGame
- WinForms/WPF
- ScintillaNET
- Any rendering or UI frameworks

This project defines data, not presentation.

---

### 3. Data First, Behavior Second

This project defines:

- what the data **is**
- how it is **parsed**
- how it is **serialized**

It should NOT define:

- how data is rendered
- how data is edited in UI
- how data is used in gameplay

---

### 4. Backwards Compatibility Preferred

When modifying formats:

- preserve compatibility where possible
- avoid breaking existing parsers unless necessary
- document breaking changes clearly

---

### 5. Reverse Engineering Guidelines

When implementing or modifying formats:

- Do not guess unknown fields
- Preserve unknown data where possible
- Clearly label incomplete or partially understood structures
- Document assumptions in code comments
- Do not hardcode game paths
- Use locals.json or environment variables
- Code must handle missing configuration gracefully

LibLR1 should prioritize correctness and completeness over convenience.

---

## Cross-Project Responsibilities

LibLR1 feeds:

- LR1 Track Editor (rendering + editing)
- LR1 Binary Editor (structured inspection)
- LR1 Online Mod (runtime hooks)
- Future Blender/export tools

Changes in this repository affect all downstream tools.

## Repository Guidelines

### Project Structure & Module Organization
`LibLR1.sln` contains two C# projects targeting .NET Framework 4.8. `LibLR1/` is the library: most top-level `*.cs` files are parsers named after LEGO Racers file types (`ADB.cs`, `GDB.cs`, `SKB.cs`). Shared helpers live in `LibLR1/IO`, `LibLR1/Utils`, and `LibLR1/Exceptions`. `Tester/` is a console harness for manual validation against real game assets.

### Build, Test, and Development Commands
Build from a Visual Studio Developer PowerShell or any shell with `msbuild` on `PATH`:

```powershell
msbuild LibLR1.sln /p:Configuration=Debug
msbuild LibLR1.sln /p:Configuration=Release
```

Use the `Tester` project to exercise parsers against installed game files. Update the hard-coded `gameFolder` in `Tester/Program.cs` first, then run the built executable from `Tester\bin\Debug\Tester.exe`.

### Coding Style & Naming Conventions
Follow the existing C# style in the repo:

- Use tabs for indentation.
- Keep braces on their own lines.
- Use `PascalCase` for types, methods, and public properties.
- Prefix method parameters with `p_` and private fields with `m_`.
- Match parser filenames and class names to the handled file extension.

No formatter or linter is configured here, so consistency with surrounding files matters more than introducing new style rules.

### Testing Guidelines
There is no separate unit-test project yet. Validation is currently integration-style: add or update parser logic in `LibLR1`, then run `Tester` against representative game files and confirm the pass count at the end. When adding support for a new format, add a corresponding `Test(gameFolder, "*.EXT", ...)` call in `Tester/Program.cs`.

### Commit & Pull Request Guidelines
Recent commits use short, imperative, sentence-case subjects such as `Add parsers for remaining file formats and update existing ones`. Keep commit messages focused and descriptive. For pull requests, include:

- A brief summary of the parser or utility changes.
- The file types or sample assets used for validation.
- Any known unsupported blocks, assumptions, or reverse-engineering notes.

Avoid committing local dump files, extracted game assets, or other machine-specific artifacts.
