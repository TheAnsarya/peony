using BenchmarkDotNet.Attributes;
using Peony.Core;
using Peony.Platform.NES;
using Peony.Platform.SNES;

namespace Peony.Benchmarks;

[MemoryDiagnoser]
public class DisassemblyPipelineBenchmarks {
	private byte[] _nesRom = null!;
	private byte[] _snesRom = null!;
	private NesAnalyzer _nesAnalyzer = null!;
	private SnesAnalyzer _snesAnalyzer = null!;

	[GlobalSetup]
	public void Setup() {
		var rng = new Random(42);

		// Create a minimal but valid-ish NES ROM with iNES header
		_nesRom = new byte[16 + 32768]; // 16-byte header + 32KB PRG
		// iNES header
		_nesRom[0] = 0x4e; // 'N'
		_nesRom[1] = 0x45; // 'E'
		_nesRom[2] = 0x53; // 'S'
		_nesRom[3] = 0x1a; // EOF
		_nesRom[4] = 0x02; // 2x 16KB PRG banks
		_nesRom[5] = 0x00; // 0 CHR banks
		rng.NextBytes(_nesRom.AsSpan(16));

		// Set reset vector to valid ROM address
		_nesRom[16 + 32768 - 4] = 0x00; // Reset low
		_nesRom[16 + 32768 - 3] = 0x80; // Reset high ($8000)

		// Create a minimal SNES ROM
		_snesRom = new byte[65536]; // 64KB
		rng.NextBytes(_snesRom);

		_nesAnalyzer = new NesAnalyzer();
		_snesAnalyzer = new SnesAnalyzer();
	}

	[Benchmark]
	public DisassemblyResult DisassembleNES_32KB() {
		var engine = new DisassemblyEngine(
			_nesAnalyzer.CpuDecoder,
			_nesAnalyzer
		);
		var entryPoints = _nesAnalyzer.GetEntryPoints(_nesRom);
		return engine.Disassemble(_nesRom, entryPoints);
	}

	[Benchmark]
	public RomInfo AnalyzeNES() {
		return _nesAnalyzer.Analyze(_nesRom);
	}

	[Benchmark]
	public uint[] GetNESEntryPoints() {
		return _nesAnalyzer.GetEntryPoints(_nesRom);
	}

	[Benchmark]
	public string? RegisterLookup_10K() {
		string? last = null;
		for (uint addr = 0x2000; addr < 0x4020; addr++) {
			last = _nesAnalyzer.GetRegisterLabel(addr);
		}
		return last;
	}
}
