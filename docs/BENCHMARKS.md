# Benchmarks

## Overview

Peony includes a `BenchmarkDotNet` benchmark suite in `benchmarks/Peony.Benchmarks/` that measures performance-critical operations:

- **OpcodeLookup** — `Dictionary` vs `FrozenDictionary` for opcode table access
- **Counting** — Single-pass switch counting vs multiple LINQ `Count()` calls
- **Materialization** — `ToArray()` vs `ToList()` for sequence materialization
- **GameBoy** — End-to-end CPU decoder throughput

## Running Benchmarks

### Recommended Invocation

Use `-p:RunAnalyzers=false` to suppress CA/IDE analyzer warnings from referenced projects. This keeps console output focused on timing data without weakening normal CI quality gates:

```powershell
# Run all benchmarks (clean output)
dotnet run --project benchmarks/Peony.Benchmarks -c Release -p:RunAnalyzers=false

# Quick validation (Dry job — single iteration, ColdStart)
dotnet run --project benchmarks/Peony.Benchmarks -c Release -p:RunAnalyzers=false -- --job dry

# Filter to specific benchmark
dotnet run --project benchmarks/Peony.Benchmarks -c Release -p:RunAnalyzers=false -- --filter "*SinglePassCount*"

# Filter by category
dotnet run --project benchmarks/Peony.Benchmarks -c Release -p:RunAnalyzers=false -- --filter "*" --anyCategories OpcodeLookup
```

### Why `-p:RunAnalyzers=false`?

The benchmark project itself already sets `<RunAnalyzers>false</RunAnalyzers>` in its `.csproj`. However, BenchmarkDotNet rebuilds the entire dependency graph (Peony.Core, Peony.Cpu.*, Peony.Platform.*), and those projects inherit `AnalysisLevel=latest-recommended` from the root `Directory.Build.props`. The `-p:` flag propagates to **all** projects in the build graph, suppressing the hundreds of CA/IDE warnings that otherwise flood benchmark output.

Normal `dotnet build` and CI builds are unaffected — analyzers remain fully enabled for regular development.

### Without the Flag

Without `-p:RunAnalyzers=false`, expect ~380+ analyzer warnings printed before benchmark results, making it difficult to read timing summaries.

## MinIterationTime Threshold

BenchmarkDotNet warns when a benchmark iteration completes in under 100ms (`MinIterationTime`). All benchmarks in the suite are tuned to exceed this threshold by scaling their `WorkMultiplier` parameter.

If you see a MinIterationTime warning after hardware changes, increase the iteration count multiplier in the affected benchmark method.

## Benchmark Project Configuration

Key settings in `benchmarks/Peony.Benchmarks/Peony.Benchmarks.csproj`:

| Setting | Value | Purpose |
|---------|-------|---------|
| `RunAnalyzers` | `false` | Suppress analyzers for the benchmark project itself |
| `RunAnalyzersDuringBuild` | `false` | Redundant safety for analyzer suppression |
| `IsPackable` | `false` | Prevent NuGet packaging |

## Results

Benchmark artifacts are written to `BenchmarkDotNet.Artifacts/results/` (gitignored). For historical results, see session logs in `~docs/session-logs/`.
