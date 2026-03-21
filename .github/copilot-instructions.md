# 🌺 Peony Disassembler - AI Copilot Directives

## Project Overview

**🌺 Peony** is a multi-system disassembler framework — the "anti-Poppy" that converts ROM binaries back into source code, generating `.pasm` files that reassemble to identical ROMs using Poppy.

**Purpose:**
- Multi-architecture disassembler (6502, 65816, 65SC02, SM83, ARM7TDMI)
- Multi-platform support (NES, SNES, GB, GBA, Atari 2600, Lynx)
- C# core library for disassembly pipeline
- CLI tool for disassembling ROM files
- Pansy metadata generation (symbols, CDL, cross-refs, memory regions)
- Roundtrip guarantee: disassembled code → Poppy → identical ROM

## GitHub Issue Management

### ⚠️ CRITICAL: Always Create Issues on GitHub Directly

**NEVER just document issues in markdown files.** Always create actual GitHub issues using the `gh` CLI:

```powershell
# Create an issue
gh issue create --repo TheAnsarya/peony --title "Issue Title" --body "Description" --label "label1,label2"

# Add labels
gh issue edit <number> --repo TheAnsarya/peony --add-label "label"

# Close issue
gh issue close <number> --repo TheAnsarya/peony --comment "Completed in commit abc123"
```

### Required Labels
- `enhancement` - New features
- `bug` - Bug fixes
- `documentation` - Docs work
- `performance` - Performance related
- `testing` - Test related
- `pansy` - Pansy integration related
- Priority: `high-priority`, `medium-priority`, `low-priority`

### ⚠️ MANDATORY: Issue-First Workflow

**Always create GitHub issues BEFORE starting implementation work.** This is non-negotiable.

1. **Before Implementation:**
   - Create a GitHub issue describing the planned work
   - Include scope, approach, and acceptance criteria
   - Add appropriate labels
   - Create plans in `~Plans/` for non-trivial work

2. **During Implementation:**
   - Reference issue number in commits: `git commit -m "Fix decoder bug - #247"`
   - Update issue with progress comments if work spans multiple sessions
   - Add sub-issues for discovered work

3. **After Implementation:**
   - Close issue with completion comment including commit hash
   - Link related issues if applicable

**Workflow Pattern:**
```powershell
# 1. Create issue FIRST
gh issue create --repo TheAnsarya/peony --title "Description" --body "Details" --label "label"

# 2. Add prompt tracking comment (for AI-created issues)
gh issue comment <number> --repo TheAnsarya/peony --body "Prompt for work:`n{original user prompt}"

# 3. Implement the fix/feature

# 4. Commit with issue reference
git add .
git commit -m "Brief description - #<issue-number>"

# 5. Close issue with summary
gh issue close <number> --repo TheAnsarya/peony --comment "Completed in <commit-hash>"
```

### ⚠️ MANDATORY: Prompt Tracking for AI-Created Issues

When creating GitHub issues from AI prompts, **IMMEDIATELY** add the original user prompt as the **FIRST comment** right after creating the issue — before doing any implementation work:

```powershell
# Create issue
$issueUrl = gh issue create --repo TheAnsarya/peony --title "Description" --body "Details" --label "label"
$issueNum = ($issueUrl -split '/')[-1]

# IMMEDIATELY add prompt as first comment (before any other work)
gh issue comment $issueNum --repo TheAnsarya/peony --body "Prompt for work:
<original user prompt that triggered this work>"
```

## Coding Standards

### Indentation
- **TABS for indentation** — enforced by `.editorconfig`
- **Tab width: 4 spaces** — ALWAYS use 4-space-equivalent tabs
- NEVER use spaces for indentation — only tabs
- Inside code blocks in markdown, use spaces for alignment of ASCII art/diagrams

### Brace Style — K&R (One True Brace)
- **Opening braces on the SAME LINE** as the statement — ALWAYS
- This applies to ALL constructs: `if`, `else`, `for`, `while`, `switch`, `try`, `catch`, functions, methods, classes, structs, namespaces, lambdas, properties, enum declarations, etc.
- `else` and `else if` go on the same line as the closing brace: `} else {`
- `catch` goes on the same line as the closing brace: `} catch (...) {`
- **NEVER use Allman style** (brace on its own line)
- **NEVER put an opening brace on a new line** — not even for long parameter lists

#### C# Examples

```csharp
// ✅ CORRECT — K&R style
if (condition) {
DoSomething();
} else if (other) {
DoOther();
} else {
DoFallback();
}

for (int i = 0; i < count; i++) {
Process(i);
}

public void Execute(int param) {
// body
}

public class MyClass : Base {
public string Name { get; set; }

public void Method() {
// body
}
}

// ❌ WRONG — Allman style (DO NOT USE)
if (condition)
{
DoSomething();
}
```

### Hexadecimal Values
- **Always lowercase**: `0xff00`, not `0xFF00`
- **Format specifiers lowercase**: `:x4`, not `:X4`
- Use `$` for addresses in output/documentation: `$ff00`

### Assembly Output
- All opcodes/operands in **lowercase** (`lda`, `sta`, `jsr`, NOT `LDA`, `STA`, `JSR`)
- All hex values in **lowercase** with `$` prefix in `.pasm` output
- Generate `.pasm` files compatible with Poppy

### C# Standard
- **.NET 10** with latest C# features
- File-scoped namespaces where applicable
- Nullable reference types enabled
- Modern pattern matching

### Encoding & Line Endings
- **UTF-8** encoding with BOM for all files
- **CRLF** line endings (Windows style)
- Support for Unicode and emojis

### ⚠️ Comment Safety Rule
**When adding or modifying comments, NEVER change the actual code.**

- Changes to comments must not alter code logic, structure, or formatting
- When adding XML documentation or inline comments, preserve all existing code exactly
- Verify code integrity after adding documentation

## Architecture

### Core Abstractions
- `ICpuDecoder` — CPU instruction decoding (6502, 65816, 65SC02, SM83, ARM7TDMI)
- `IPlatformAnalyzer` — Platform-specific analysis (NES, SNES, GB, GBA, Atari 2600, Lynx)
- `IOutputFormatter` — Output generation (Poppy `.pasm` format)
- `DisassemblyEngine` — Main disassembly pipeline
- `SymbolExporter` — Exports symbols/metadata to Pansy and other formats
- `SymbolLoader` — Loads symbols/metadata from Pansy files (uses `Pansy.Core.PansyLoader`)
- `CdlLoader` — Loads CDL (Code/Data Log) files
- `RoundtripVerifier` — Verifies disassembly→assembly roundtrip

### Roundtrip Guarantee
**CRITICAL**: All disassembled code MUST reassemble to identical ROMs using Poppy.

## Pansy Integration

Peony both consumes and produces Pansy metadata files (`.pansy`):

### Reading (SymbolLoader)
- Uses `Pansy.Core.PansyLoader` — the canonical reader
- Loads symbols, comments, CDL data, memory regions, cross-refs
- Feeds hints into `DisassemblyEngine` for better analysis

### Writing (SymbolExporter)
- **MUST use `Pansy.Core.PansyWriter`** — the canonical writer
- Never reimplement binary serialization manually
- Generates all supported sections:

### Pansy Sections
1. **Code/Data Map** (0x0001) — Per-byte CDL flags
2. **Symbols** (0x0002) — Address → Name + Type
3. **Comments** (0x0003) — Address → Text + Type
4. **Memory Regions** (0x0004) — Named memory regions with types and banks
5. **Cross-References** (0x0006) — From/To address pairs with type
6. **Metadata** (0x0008) — Project name, author, version

### CDL Flags
- CODE=0x01, DATA=0x02, JUMP_TARGET=0x04, SUB_ENTRY=0x08
- OPCODE=0x10, DRAWN=0x20, READ=0x40, INDIRECT=0x80

### CrossRefType (Pansy Spec)
- Jsr=1, Jmp=2, Branch=3, Read=4, Write=5

### SymbolTypes
- Label=1, Constant=2, Enum=3, Struct=4, Macro=5
- Local=6, Anonymous=7, InterruptVector=8, Function=9

### CommentTypes
- Inline=1, Block=2, Todo=3

### MemoryRegionType
- ROM=1, RAM=2, VRAM=3, IO=4, SRAM=5, WRAM=6, OpenBus=7, Mirror=8

### Platform IDs
- NES=0x01, SNES=0x02, GB=0x03, GBA=0x04, Genesis=0x05
- SMS=0x06, PCE=0x07, Atari2600=0x08, Lynx=0x09, WonderSwan=0x0a
- Custom=0xff

### Compression
- DEFLATE per-section (not GZip)
- Set FLAG_COMPRESSED in header flags

## Testing Guidelines

### ⚠️ MANDATORY: Before/After Testing

**EVERY code change MUST include before/after test runs.** This is non-negotiable.

1. **Before any code change:** Run the full test suite and record pass/fail counts
2. **After the change:** Run the full test suite — ALL tests must pass
3. **Add new tests** for any new functionality or bug fixes
4. **Never commit with failing tests**

```powershell
# Run all tests
dotnet test Peony.sln -c Release --nologo -v m

# Run specific test project
dotnet test tests/Peony.Core.Tests -c Release --nologo

# Run specific test class
dotnet test tests/Peony.Core.Tests -c Release --filter "ClassName=PansyExportTests"
```

### Test Categories
- **CPU decoder tests** — Instruction decoding correctness per architecture
- **Platform tests** — Platform-specific analysis (memory maps, I/O, vectors)
- **Pansy export tests** — Symbol/CDL/cross-ref export to Pansy format
- **Pansy import tests** — Loading Pansy hints into disassembler
- **Roundtrip tests** — Disassemble → Poppy assemble → compare
- **Integration tests** — Full pipeline tests with real ROMs

### Verification Checklist (for EVERY code change):
1. ✅ All tests pass (`dotnet test Peony.sln -c Release`)
2. ✅ Build succeeds (`dotnet build Peony.sln -c Release`)
3. ✅ New tests added for new/changed functionality
4. ✅ No new warnings in build output
5. ✅ Code formatted (tabs, K&R braces, lowercase hex)

## Build Commands

```powershell
# Build entire solution
dotnet build Peony.sln -c Release

# Run all tests
dotnet test Peony.sln -c Release --nologo -v m

# Run CLI
dotnet run --project src/Peony.Cli -- <args>
```

## 📁 Project Structure

```
/                              # Root
├── .github/                   # GitHub configuration
├── docs/                      # User documentation
├── output/                    # Generated .pasm output files
├── src/                       # Source code
│   ├── Peony.Core/            # Core disassembly library
│   │   ├── DisassemblyEngine.cs
│   │   ├── SymbolExporter.cs  # Pansy/Diz/Label export
│   │   ├── SymbolLoader.cs    # Pansy/Diz/Label import
│   │   ├── CdlLoader.cs      # CDL file loading
│   │   ├── Interfaces.cs     # Core abstractions
│   │   ├── RomLoader.cs
│   │   └── RoundtripVerifier.cs
│   ├── Peony.Cli/             # Command-line interface
│   ├── Peony.Cpu.6502/        # MOS 6502 decoder (NES)
│   ├── Peony.Cpu.65816/       # WDC 65816 decoder (SNES)
│   ├── Peony.Cpu.65SC02/      # 65SC02 decoder (Lynx)
│   ├── Peony.Cpu.ARM7TDMI/    # ARM7TDMI decoder (GBA)
│   ├── Peony.Cpu.SM83/        # Sharp SM83 decoder (GB)
│   ├── Peony.Cpu.GameBoy/     # Legacy GB decoder
│   ├── Peony.Platform.NES/    # NES platform analyzer
│   ├── Peony.Platform.SNES/   # SNES platform analyzer
│   ├── Peony.Platform.GB/     # Game Boy platform
│   ├── Peony.Platform.GBA/    # GBA platform
│   ├── Peony.Platform.Atari2600/ # Atari 2600 platform
│   └── Peony.Platform.Lynx/   # Atari Lynx platform
├── tests/                     # Test projects
│   ├── Peony.Core.Tests/
│   ├── Peony.Cpu.6502.Tests/
│   ├── Peony.Platform.Atari2600.Tests/
│   ├── Peony.Platform.GameBoy.Tests/
│   ├── Peony.Platform.GBA.Tests/
│   ├── Peony.Platform.Lynx.Tests/
│   └── Peony.Platform.SNES.Tests/
├── ~docs/                     # Project documentation
│   ├── chat-logs/             # AI conversation logs
│   └── session-logs/          # Session summaries
├── ~Plans/                    # Technical plans
├── ~manual-testing/           # Manual test files
└── ~reference-files/          # Reference materials
```

## Git Workflow

### ⚠️ MANDATORY: Format Before Every Commit

Before EVERY commit:
1. Verify tab indentation (no spaces)
2. Verify K&R brace style (no Allman)
3. Verify lowercase hex (`0xff`, `:x4` not `0xFF`, `:X4`)
4. Run build to check for warnings
5. Run tests to verify correctness

### Commit Messages
- **Always reference issue numbers**: `Brief description - #<issue-number>`
- Logical, atomic commits — one concern per commit
- Use conventional prefixes: `feat:`, `fix:`, `test:`, `docs:`, `perf:`, `refactor:`

### Branching
- Create feature branches for significant work
- Branch naming: `feature/description`, `fix/description`
- Merge back to `main` when complete

## Documentation

- Session logs: `~docs/session-logs/YYYY-MM-DD-session-NN.md`
- Plans: `~Plans/`
- User docs: `docs/`
- All docs should be reachable from `README.md`

## Problem-Solving Philosophy

### ⚠️ NEVER GIVE UP on Hard Problems

When a task is complex or seems difficult:

1. **NEVER declare something "too hard" or "not worth it"** and close the issue
2. **Break it down** — Create multiple smaller sub-issues for research, prototyping, and incremental progress
3. **Research first** — Create research issues to investigate approaches, alternatives, and prior art
4. **Document everything** — Create docs, code-plans, and analysis documents in `~Plans/`
5. **Prototype** — Create spike/prototype branches to test approaches before committing
6. **Incremental progress** — Even partial progress (e.g., replacing 3 of 15 usages) is valuable
7. **Create issues for future work** — If something can't be done now, create well-documented issues for later

### Issue Decomposition Pattern
When facing a large task:
- `Research/Investigation` — Analyze scope, dependencies, alternatives
- `Document findings` — Create technical analysis docs
- `Create prototype` — Spike branch to test feasibility
- `Implement Phase 1` — Lowest-risk subset first
- `Implement Phase 2` — Next batch of changes
- `Final cleanup` — Remove old code, update docs

## Related Projects

- **Pansy** — Metadata format for disassembly data (`.pansy` files)
- **Poppy** — Assembler (source → ROM, `.pasm` files)
- **Nexen** — Multi-system emulator (exports Pansy metadata)
- **GameInfo** — ROM hacking toolkit
- **BPS-Patch** — Binary patching system

## ⚠️ Important Notes

1. **Never use spaces for indentation** — TABS ONLY
2. **Never use uppercase hex** — always lowercase (`0xff`, `:x4`)
3. **Always** add BOM to UTF-8 files
4. **Always** ensure documentation is linked from README
5. **Always use `.pasm` file extension** for disassembly output — Poppy Assembly format
6. **Always use Pansy.Core** for Pansy file I/O — never reimplement binary format
7. **Always** create GitHub issues before starting work
8. **Always** run tests before and after code changes
9. **Always** format code before committing (tabs, K&R, lowercase hex)
10. **Always** tie commits to issues with `#<number>` references
11. **Always** verify roundtrip guarantee for disassembly changes

## Markdown Formatting

### ⚠️ MANDATORY: Fix Markdownlint Warnings

**Always fix markdownlint warnings when editing or creating markdown files.** This is non-negotiable.

Key rules to enforce:

- **MD022** — Blank lines above and below headings
- **MD031** — Blank lines around fenced code blocks
- **MD032** — Blank lines around lists (ordered and unordered)
- **MD047** — Files must end with a single newline character
- **MD010** — Disabled (hard tabs are REQUIRED per our indentation rules)

When generating new markdown content, **always include proper blank line spacing** around headings, lists, and code blocks.

### ⚠️ MANDATORY: Documentation Link-Tree

**Every markdown file in the repository must be reachable from `README.md` through a hierarchical link structure.**

- The main `README.md` must link to all documentation directories and key files
- Subdirectory docs should link back to parent and to sibling docs
- No orphan markdown files — if a `.md` file exists, it must be discoverable from the root README
- When adding new documentation, always update `README.md` with a link to it
- Internal docs (`~docs/`) should have their own index linked from the main README
