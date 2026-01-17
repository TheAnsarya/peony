namespace Peony.Core;

/// <summary>
/// CPU instruction decoder interface
/// </summary>
public interface ICpuDecoder {
/// <summary>CPU architecture name (e.g., "6502", "65816")</summary>
string Architecture { get; }

/// <summary>Decode instruction at offset</summary>
DecodedInstruction Decode(ReadOnlySpan<byte> data, uint address);

/// <summary>Check if instruction modifies control flow</summary>
bool IsControlFlow(DecodedInstruction instruction);

/// <summary>Get possible target addresses for control flow instruction</summary>
IEnumerable<uint> GetTargets(DecodedInstruction instruction, uint address);
}

/// <summary>
/// Platform-specific ROM analyzer
/// </summary>
public interface IPlatformAnalyzer {
/// <summary>Platform name (e.g., "Atari 2600", "NES")</summary>
string Platform { get; }

/// <summary>Associated CPU decoder</summary>
ICpuDecoder CpuDecoder { get; }

/// <summary>Number of PRG banks (for multi-bank ROMs)</summary>
int BankCount { get; }

/// <summary>Detect ROM type and configuration</summary>
RomInfo Analyze(ReadOnlySpan<byte> rom);

/// <summary>Get hardware register label for address</summary>
string? GetRegisterLabel(uint address);

/// <summary>Get memory region type for address</summary>
MemoryRegion GetMemoryRegion(uint address);

/// <summary>Get entry points (reset vectors, etc.) from ROM</summary>
uint[] GetEntryPoints(ReadOnlySpan<byte> rom);

/// <summary>Convert CPU address to file offset (uses default/last bank)</summary>
int AddressToOffset(uint address, int romLength);

/// <summary>Convert CPU address to file offset for specific bank</summary>
int AddressToOffset(uint address, int romLength, int bank);

/// <summary>Check if address is in switchable bank region</summary>
bool IsInSwitchableRegion(uint address);

/// <summary>Detect bank switch calls (e.g., BRK-based)</summary>
BankSwitchInfo? DetectBankSwitch(ReadOnlySpan<byte> rom, uint address, int currentBank);
}

/// <summary>
/// Output format generator
/// </summary>
public interface IOutputFormatter {
/// <summary>Format name</summary>
string Name { get; }

/// <summary>File extension</summary>
string Extension { get; }

/// <summary>Generate output from disassembly</summary>
void Generate(DisassemblyResult result, string outputPath);
}

/// <summary>
/// Decoded instruction
/// </summary>
public record DecodedInstruction(
string Mnemonic,
string Operand,
byte[] Bytes,
AddressingMode Mode
);

/// <summary>
/// Bank switch information
/// </summary>
public record BankSwitchInfo(
int TargetBank,
uint TargetAddress,
string? FunctionName
);

/// <summary>
/// Addressing modes
/// </summary>
public enum AddressingMode {
Implied,
Immediate,
ZeroPage,
ZeroPageX,
ZeroPageY,
Absolute,
AbsoluteX,
AbsoluteY,
Indirect,
IndirectX,
IndirectY,
Relative
}

/// <summary>
/// Memory region types
/// </summary>
public enum MemoryRegion {
Unknown,
Code,
Data,
Graphics,
Audio,
Ram,
Rom,
Hardware
}

/// <summary>
/// ROM information
/// </summary>
public record RomInfo(
string Platform,
int Size,
string? Mapper,
Dictionary<string, string> Metadata
);

/// <summary>
/// Disassembly result
/// </summary>
public class DisassemblyResult {
public RomInfo RomInfo { get; set; } = null!;
public List<DisassembledBlock> Blocks { get; } = [];
public Dictionary<uint, string> Labels { get; } = [];
public Dictionary<(uint Address, int Bank), string> BankLabels { get; } = [];
public Dictionary<uint, string> Comments { get; } = [];
public Dictionary<int, List<DisassembledBlock>> BankBlocks { get; } = [];

/// <summary>
/// Get label for an address, checking bank-specific labels first if bank is provided.
/// </summary>
public string? GetLabel(uint address, int? bank = null) {
	// Try bank-specific label first
	if (bank.HasValue && BankLabels.TryGetValue((address, bank.Value), out var bankLabel))
		return bankLabel;
	// Fall back to global label
	return Labels.GetValueOrDefault(address);
}
}

/// <summary>
/// Block of disassembled code/data
/// </summary>
public record DisassembledBlock(
uint StartAddress,
uint EndAddress,
MemoryRegion Type,
List<DisassembledLine> Lines,
int Bank = -1
);

/// <summary>
/// Single disassembled line
/// </summary>
public record DisassembledLine(
uint Address,
byte[] Bytes,
string? Label,
string Content,
string? Comment,
int Bank = -1
);
