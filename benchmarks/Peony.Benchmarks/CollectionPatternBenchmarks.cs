using System.Collections.Frozen;
using BenchmarkDotNet.Attributes;
using Peony.Cpu.GameBoy;

namespace Peony.Benchmarks;

/// <summary>
/// Benchmarks comparing FrozenDictionary vs Dictionary for opcode lookups,
/// and single-pass vs multi-pass counting patterns.
/// Validates performance improvements from #120 and #121.
/// </summary>
[MemoryDiagnoser]
public class CollectionPatternBenchmarks {
	// --- Opcode lookup data ---
	private Dictionary<byte, (string mnemonic, int bytes)> _mutableOpcodes = null!;
	private FrozenDictionary<byte, (string mnemonic, int bytes)> _frozenOpcodes = null!;
	private byte[] _opcodeStream = null!;

	// --- Counting data ---
	private (string Name, int Type)[] _entries = null!;

	// --- GameBoy decoder (real FrozenDictionary) ---
	private GameBoyCpuDecoder _decoder = null!;
	private byte[] _gbRom = null!;

	[GlobalSetup]
	public void Setup() {
		// Simulate opcode table (90 entries, byte keys)
		var dict = new Dictionary<byte, (string mnemonic, int bytes)>();
		for (int i = 0; i < 90; i++) {
			dict[(byte)(i * 2 + 6)] = ($"op_{i}", (i % 3) + 1);
		}
		_mutableOpcodes = dict;
		_frozenOpcodes = dict.ToFrozenDictionary();

		// Random opcode stream (80% hits, 20% misses)
		var rng = new Random(42);
		_opcodeStream = new byte[50000];
		for (int i = 0; i < _opcodeStream.Length; i++) {
			_opcodeStream[i] = (byte)rng.Next(0, 256);
		}

		// Counting data (simulates DizLoader stats categories)
		_entries = new (string, int)[5000];
		for (int i = 0; i < _entries.Length; i++) {
			_entries[i] = ($"sym_{i}", i % 4); // 4 types: 0=Label, 1=Constant, 2=Enum, 3=Other
		}

		// Real GameBoy decoder for end-to-end benchmark
		_decoder = new GameBoyCpuDecoder();
		_gbRom = new byte[32768];
		rng.NextBytes(_gbRom);
	}

	// ===== FrozenDictionary vs Dictionary opcode lookup =====

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("OpcodeLookup")]
	public int Dictionary_TryGetValue_Opcodes() {
		int found = 0;
		var dict = _mutableOpcodes;
		foreach (byte op in _opcodeStream) {
			if (dict.TryGetValue(op, out _))
				found++;
		}
		return found;
	}

	[Benchmark]
	[BenchmarkCategory("OpcodeLookup")]
	public int FrozenDictionary_TryGetValue_Opcodes() {
		int found = 0;
		var dict = _frozenOpcodes;
		foreach (byte op in _opcodeStream) {
			if (dict.TryGetValue(op, out _))
				found++;
		}
		return found;
	}

	// ===== Single-pass count vs multiple LINQ Count() =====

	[Benchmark]
	[BenchmarkCategory("Counting")]
	public (int, int, int, int) MultipleLinqCount() {
		int labels = _entries.Count(e => e.Type == 0);
		int constants = _entries.Count(e => e.Type == 1);
		int enums = _entries.Count(e => e.Type == 2);
		int other = _entries.Count(e => e.Type == 3);
		return (labels, constants, enums, other);
	}

	[Benchmark]
	[BenchmarkCategory("Counting")]
	public (int, int, int, int) SinglePassCount() {
		int labels = 0, constants = 0, enums = 0, other = 0;
		foreach (var entry in _entries) {
			switch (entry.Type) {
				case 0: labels++; break;
				case 1: constants++; break;
				case 2: enums++; break;
				case 3: other++; break;
			}
		}
		return (labels, constants, enums, other);
	}

	// ===== ToArray vs ToList =====

	[Benchmark]
	[BenchmarkCategory("Materialization")]
	public List<(string, int)> OrderBy_ToList() {
		return _entries.OrderBy(e => e.Name).ToList();
	}

	[Benchmark]
	[BenchmarkCategory("Materialization")]
	public (string, int)[] OrderBy_ToArray() {
		return _entries.OrderBy(e => e.Name).ToArray();
	}

	// ===== Real GameBoy decoder throughput =====

	[Benchmark]
	[BenchmarkCategory("Decoder")]
	public int GameBoyCpuDecoder_Decode32KB() {
		int count = 0;
		int offset = 0;
		while (offset < _gbRom.Length) {
			var instr = _decoder.Decode(_gbRom.AsSpan(offset), (uint)offset);
			offset += Math.Max(1, instr.Bytes.Length);
			count++;
		}
		return count;
	}
}
