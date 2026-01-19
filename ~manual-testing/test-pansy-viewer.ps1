# Test script for Pansy viewer command

# Create a minimal test ROM file (NES header + some data)
$romPath = "test.nes"
$pansyPath = "test.pansy"

# Create a minimal NES ROM (16 byte header + 16KB PRG)
$header = [byte[]]@(
	0x4E, 0x45, 0x53, 0x1A,  # "NES" + MS-DOS EOF
	0x01,                    # 1 x 16KB PRG-ROM
	0x01,                    # 1 x 8KB CHR-ROM
	0x00,                    # Mapper 0
	0x00,                    # Mapper 0
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
)

$prg = New-Object byte[] (16384)
for ($i = 0; $i -lt 16384; $i++) {
	$prg[$i] = [byte]($i % 256)
}

# Write interrupt vectors at end of ROM
$prg[16380] = 0x00  # NMI vector low
$prg[16381] = 0x80  # NMI vector high
$prg[16382] = 0x00  # RESET vector low
$prg[16383] = 0x80  # RESET vector high

[System.IO.File]::WriteAllBytes($romPath, $header + $prg)

Write-Host "Created test ROM: $romPath" -ForegroundColor Green

# Now export to Pansy
Write-Host "`nExporting to Pansy format..." -ForegroundColor Cyan
dotnet run --project ../src/Peony.Cli -- export $romPath -o $pansyPath -f pansy

if (Test-Path $pansyPath) {
	Write-Host "`nPansy file created: $pansyPath" -ForegroundColor Green

	# Test the viewer
	Write-Host "`n=== Testing Pansy Viewer ===" -ForegroundColor Magenta
	dotnet run --project ../src/Peony.Cli -- pansy $pansyPath

	Write-Host "`n=== Testing Verbose Mode ===" -ForegroundColor Magenta
	dotnet run --project ../src/Peony.Cli -- pansy $pansyPath --verbose
}
else {
	Write-Host "Failed to create Pansy file" -ForegroundColor Red
}

# Cleanup
Remove-Item $romPath -ErrorAction SilentlyContinue
Remove-Item $pansyPath -ErrorAction SilentlyContinue
