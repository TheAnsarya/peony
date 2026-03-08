using BenchmarkDotNet.Attributes;
using Pansy.Core;
using Peony.Core;

namespace Peony.Benchmarks;

[MemoryDiagnoser]
public class SymbolExportBenchmarks {
	private DisassemblyResult _result = null!;
	private byte[] _pansyBytes = null!;

	[GlobalSetup]
	public void Setup() {
		_result = new DisassemblyResult {
			RomInfo = new RomInfo("NES", 32768, "NROM", new Dictionary<string, string>())
		};

		// Add 1000 labels
		for (uint i = 0; i < 1000; i++) {
			_result.Labels[0x8000 + i] = $"sub_{0x8000 + i:x4}";
		}

		// Add 500 comments
		for (uint i = 0; i < 500; i++) {
			_result.Comments[0x8000 + i * 2] = $"Function at ${0x8000 + i * 2:x4}";
		}

		// Add cross-references
		for (uint i = 0; i < 200; i++) {
			var addr = 0x8000u + i * 4;
			_result.CrossReferences[addr] = [
				new CrossRef(addr - 0x10, 0, Peony.Core.CrossRefType.Call),
				new CrossRef(addr - 0x20, 0, Peony.Core.CrossRefType.Jump)
			];
		}

		// Pre-generate bytes for import benchmark
		_pansyBytes = CreatePansyBytes();
	}

	private byte[] CreatePansyBytes() {
		var writer = new PansyWriter {
			Platform = PansyLoader.PLATFORM_NES,
			RomSize = 32768,
			RomCrc32 = 0xdeadbeef
		};

		var symbols = _result.Labels.Select(kvp =>
			(kvp.Key, kvp.Value, SymbolType.Label));
		writer.AddSymbols(symbols);

		var comments = _result.Comments.Select(kvp =>
			(kvp.Key, kvp.Value, CommentType.Inline));
		writer.AddComments(comments);

		return writer.Generate();
	}

	[Benchmark]
	public byte[] ExportPansyFormat() {
		var writer = new PansyWriter {
			Platform = PansyLoader.PLATFORM_NES,
			RomSize = 32768,
			RomCrc32 = 0xdeadbeef
		};

		var symbols = _result.Labels.Select(kvp =>
			(kvp.Key, kvp.Value, SymbolType.Label));
		writer.AddSymbols(symbols);

		var comments = _result.Comments.Select(kvp =>
			(kvp.Key, kvp.Value, CommentType.Inline));
		writer.AddComments(comments);

		return writer.Generate();
	}

	[Benchmark]
	public PansyLoader ImportPansyFormat() {
		return new PansyLoader(_pansyBytes);
	}

	[Benchmark]
	public string ExportMesenFormat() {
		return SymbolExporter.ExportMesen(_result);
	}
}
