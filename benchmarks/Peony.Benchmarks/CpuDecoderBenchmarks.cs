using BenchmarkDotNet.Attributes;
using Peony.Core;
using Peony.Cpu;
using Peony.Cpu.ARM7TDMI;
using Peony.Cpu.GameBoy;

namespace Peony.Benchmarks;

[MemoryDiagnoser]
public class CpuDecoderBenchmarks {
	private byte[] _nes6502Rom = null!;
	private byte[] _gbRom = null!;
	private byte[] _armRom = null!;
	private Cpu6502Decoder _cpu6502 = null!;
	private Cpu65816Decoder _cpu65816 = null!;
	private GameBoyCpuDecoder _gbDecoder = null!;
	private Arm7TdmiDecoder _armDecoder = null!;

	[GlobalSetup]
	public void Setup() {
		var rng = new Random(42);

		// Generate realistic ROM data patterns
		_nes6502Rom = new byte[32768]; // 32KB NES PRG
		rng.NextBytes(_nes6502Rom);
		// Inject some common 6502 patterns
		for (int i = 0; i < _nes6502Rom.Length - 3; i += 4) {
			_nes6502Rom[i] = (byte)(i % 256); // varied opcodes
		}

		_gbRom = new byte[32768]; // 32KB GB ROM
		rng.NextBytes(_gbRom);

		_armRom = new byte[65536]; // 64KB GBA ROM
		rng.NextBytes(_armRom);

		_cpu6502 = new Cpu6502Decoder();
		_cpu65816 = new Cpu65816Decoder();
		_gbDecoder = new GameBoyCpuDecoder();
		_armDecoder = new Arm7TdmiDecoder();
	}

	[Benchmark(Baseline = true)]
	public int Decode6502_32KB() {
		int count = 0;
		var rom = _nes6502Rom.AsSpan();
		uint addr = 0x8000;

		for (int offset = 0; offset < rom.Length - 3; offset++) {
			var instr = _cpu6502.Decode(rom[offset..], addr);
			count += instr.Bytes.Length;
			addr++;
		}

		return count;
	}

	[Benchmark]
	public int Decode65816_32KB() {
		int count = 0;
		var rom = _nes6502Rom.AsSpan();
		uint addr = 0x8000;

		for (int offset = 0; offset < rom.Length - 4; offset++) {
			var instr = _cpu65816.Decode(rom[offset..], addr);
			count += instr.Bytes.Length;
			addr++;
		}

		return count;
	}

	[Benchmark]
	public int DecodeGameBoy_32KB() {
		int count = 0;
		var rom = _gbRom.AsSpan();
		uint addr = 0x0000;

		for (int offset = 0; offset < rom.Length - 3; offset++) {
			var instr = _gbDecoder.Decode(rom[offset..], addr);
			count += instr.Bytes.Length;
			addr++;
		}

		return count;
	}

	[Benchmark]
	public int DecodeARM7_64KB() {
		int count = 0;
		var rom = _armRom.AsSpan();
		uint addr = 0x08000000;

		for (int offset = 0; offset < rom.Length - 4; offset += 4) {
			var instr = _armDecoder.Decode(rom[offset..], addr);
			count += instr.Bytes.Length;
			addr += 4;
		}

		return count;
	}
}
