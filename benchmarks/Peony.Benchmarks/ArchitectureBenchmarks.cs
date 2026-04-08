using BenchmarkDotNet.Attributes;
using Peony.Core;
using Peony.Platform.NES;
using Peony.Platform.SNES;
using Peony.Platform.GameBoy;
using Peony.Platform.GBA;
using Peony.Platform.Atari2600;
using Peony.Platform.Lynx;

namespace Peony.Benchmarks;

/// <summary>
/// Benchmarks for the architecture separation layer: PlatformResolver lookups,
/// profile access, and AsmFormatter output generation.
/// Covers #146 epic / #150 benchmarks.
/// </summary>
[MemoryDiagnoser]
public class ArchitectureBenchmarks {
	private DisassemblyResult _smallResult = null!;
	private DisassemblyResult _largeResult = null!;

	[GlobalSetup]
	public void Setup() {
		// Ensure all platforms are registered
		PlatformResolver.Clear();
		Peony.Platform.NES.Registration.RegisterAll();
		Peony.Platform.SNES.Registration.RegisterAll();
		Peony.Platform.GameBoy.Registration.RegisterAll();
		Peony.Platform.GBA.Registration.RegisterAll();
		Peony.Platform.Atari2600.Registration.RegisterAll();
		Peony.Platform.Lynx.Registration.RegisterAll();

		// Small result: 10 labels, 1 block, 5 lines
		_smallResult = new DisassemblyResult {
			RomInfo = new RomInfo("NES", 32768, "NROM", new Dictionary<string, string>())
		};
		for (uint i = 0; i < 10; i++)
			_smallResult.Labels[0x8000 + i * 0x100] = $"label_{i:x2}";
		var smallLines = new List<DisassembledLine>();
		for (uint i = 0; i < 5; i++)
			smallLines.Add(new DisassembledLine(0x8000 + i, [0xea], null, "nop", null));
		_smallResult.Blocks.Add(new DisassembledBlock(0x8000, 0x8004, MemoryRegion.Code, smallLines));

		// Large result: 1000 labels, 10 blocks, 500 lines
		_largeResult = new DisassemblyResult {
			RomInfo = new RomInfo("SNES", 262144, "LoROM", new Dictionary<string, string>())
		};
		for (uint i = 0; i < 1000; i++)
			_largeResult.Labels[0x8000 + i * 4] = $"sub_{i:x4}";
		var rng = new Random(42);
		for (int b = 0; b < 10; b++) {
			var lines = new List<DisassembledLine>();
			uint baseAddr = (uint)(0x8000 + b * 0x1000);
			for (int l = 0; l < 50; l++) {
				uint addr = baseAddr + (uint)l;
				byte op = (byte)rng.Next(256);
				lines.Add(new DisassembledLine(addr, [op], null, $"lda ${addr + 0x100:x4}", null));
			}
			_largeResult.Blocks.Add(new DisassembledBlock(
				baseAddr, baseAddr + 49, MemoryRegion.Code, lines, b));
		}
	}

	// =========================================================================
	// PlatformResolver benchmarks
	// =========================================================================

	[Benchmark]
	[BenchmarkCategory("Resolver")]
	public IPlatformProfile? ResolveName_Hit() {
		return PlatformResolver.Resolve("nes");
	}

	[Benchmark]
	[BenchmarkCategory("Resolver")]
	public IPlatformProfile? ResolveName_Miss() {
		return PlatformResolver.Resolve("nonexistent");
	}

	[Benchmark]
	[BenchmarkCategory("Resolver")]
	public IPlatformProfile? ResolveExtension_Hit() {
		return PlatformResolver.ResolveByExtension(".sfc");
	}

	[Benchmark]
	[BenchmarkCategory("Resolver")]
	public IPlatformProfile? ResolveExtension_Miss() {
		return PlatformResolver.ResolveByExtension(".xyz");
	}

	[Benchmark]
	[BenchmarkCategory("Resolver")]
	public IPlatformProfile GetProfile_ById() {
		return PlatformResolver.GetProfile(PlatformId.NES);
	}

	[Benchmark]
	[BenchmarkCategory("Resolver")]
	public int GetAll_Enumerate() {
		return PlatformResolver.GetAll().Count;
	}

	// =========================================================================
	// AsmFormatter benchmarks
	// =========================================================================

	[Benchmark]
	[BenchmarkCategory("Formatter")]
	public string FormatWithLabels_SmallResult() {
		return AsmFormatter.FormatWithLabels("jsr $8000", _smallResult);
	}

	[Benchmark]
	[BenchmarkCategory("Formatter")]
	public string FormatWithLabels_LargeResult() {
		return AsmFormatter.FormatWithLabels("jsr $8000", _largeResult);
	}

	[Benchmark]
	[BenchmarkCategory("Formatter")]
	public string FormatWithLabels_NoMatch() {
		return AsmFormatter.FormatWithLabels("nop", _smallResult);
	}

	[Benchmark]
	[BenchmarkCategory("Formatter")]
	public string WriteOutput_Small() {
		using var sw = new StringWriter();
		AsmFormatter.Instance.WriteOutput(sw, _smallResult, "test.nes");
		return sw.ToString();
	}

	[Benchmark]
	[BenchmarkCategory("Formatter")]
	public string WriteOutput_Large() {
		using var sw = new StringWriter();
		AsmFormatter.Instance.WriteOutput(sw, _largeResult, "test.sfc");
		return sw.ToString();
	}
}
