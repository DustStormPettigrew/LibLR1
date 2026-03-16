# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

LibLR1 is a C# (.NET Framework 4.8) library for parsing binary file formats from the LEGO Racers game (1999). It reverse-engineers the game's proprietary compressed binary format and exposes parsed data via structured objects.

## Build Commands

```bash
# Build the full solution
dotnet build LibLR1.sln

# Build specific project
dotnet build LibLR1/LibLR1.csproj
dotnet build Tester/Tester.csproj

# Release build
dotnet build LibLR1.sln --configuration Release
```

## Running Tests

There is no unit test framework. The `Tester/` project is a manual test harness:

```bash
dotnet run --project Tester/Tester.csproj
```

The Tester loads actual LEGO Racers game files from a hardcoded path (`E:\Games\LEGO Racers`). To test specific file formats, comment out unwanted `Test()` calls in [Tester/Program.cs](Tester/Program.cs).

## Architecture

### Data Flow

```
Game File (.ADB, .GDB, etc.)
    ↓
BinaryFileHelper.Decompress()   [LibLR1/Utils/BinaryFileHelper.cs]
    ↓
LRBinaryReader (decompressed)   [LibLR1/IO/LRBinaryReader.cs]
    ↓
Parser class (e.g. ADB, GDB)   [LibLR1/<name>.cs]
    ↓
Public properties on the parser object
```

### Token-Based Binary Format

All binary data is prefixed with type tokens (defined in [LibLR1/Utils/Token.cs](LibLR1/Utils/Token.cs)). `BinaryFileHelper.Decompress()` recursively decompresses the entire file into a flat `LRBinaryReader` stream before parsers read it. Tokens include primitives (`Int32`, `Float`, `String`, `Byte`, `Short`), structural markers (`LeftCurly`/`RightCurly`, `LeftBracket`/`RightBracket`), and compound types (`Token.Array`, `Token.Struct`).

### Key Components

- **[LibLR1/IO/LRBinaryReader.cs](LibLR1/IO/LRBinaryReader.cs)** — Custom `BinaryReader` with typed reads (`ReadToken()`, `ReadIntegralWithHeader()`, `ReadFloatWithHeader()`, etc.) that validate expected token markers.
- **[LibLR1/IO/LRBinaryWriter.cs](LibLR1/IO/LRBinaryWriter.cs)** — Symmetric writer for serialization.
- **[LibLR1/Utils/BinaryFileHelper.cs](LibLR1/Utils/BinaryFileHelper.cs)** — Static decompression logic; entry point for all file parsing.
- **[LibLR1/Utils/](LibLR1/Utils/)** — Shared data structures: `LRVector2`, `LRVector3`, `LRQuaternion`, `LRColor`, `LRRect`, `Fract8Bit`, `Fract16Bit`. Each has static `Read()`/`Write()` helpers.
- **[LibLR1/Exceptions/](LibLR1/Exceptions/)** — `UnexpectedBlockException`, `UnexpectedTypeException`, `UnexpectedPropertyException`.
- **26 parser classes** in `LibLR1/` (one per file format: `ADB`, `BDB`, `BMP`, `GDB`, `MDB`, `WDB`, etc.).

### Adding a New Parser

Follow the pattern of existing parsers: accept a file path or `LRBinaryReader`, call `BinaryFileHelper.Decompress()` if needed, then read fields using `LRBinaryReader` typed methods. Mirror read logic with write logic in `LRBinaryWriter` for serialization support.
