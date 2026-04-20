<#
.SYNOPSIS
	FFMQ disassembly regression benchmark.
	Verifies that Peony produces expected metrics when disassembling
	Final Fantasy Mystic Quest with Nexen CDL/Pansy metadata.

.DESCRIPTION
	Runs Peony disassembly on FFMQ and checks output metrics against
	known-good baselines. Fails if metrics regress below minimum thresholds.

.NOTES
	Issue: TheAnsarya/peony#186 (Epic #179)
	ROM: FFMQ (SNES LoROM, 512KB, CRC32: 2c52c792)
	Requires: ffmq.smc, ffmq-nexen.pansy, ffmq-nexen-coverage.cdl
#>
param(
	[string]$RomPath = "C:\~reference-roms\snes\ffmq.smc",
	[string]$PansyPath = "C:\Users\me\source\repos\game-garden\games\snes\ffmq\metadata\ffmq-nexen.pansy",
	[string]$CdlPath = "C:\Users\me\source\repos\game-garden\games\snes\ffmq\metadata\ffmq-nexen-coverage.cdl",
	[string]$OutputPath = "$env:TEMP\ffmq-benchmark.pasm"
)

$ErrorActionPreference = "Stop"

Write-Host "=== FFMQ Disassembly Regression Benchmark ===" -ForegroundColor Cyan
Write-Host ""

# Verify inputs exist
foreach ($file in @($RomPath, $PansyPath, $CdlPath)) {
	if (-not (Test-Path $file)) {
		Write-Host "MISSING: $file" -ForegroundColor Red
		exit 1
	}
}

# Run disassembly
Write-Host "Running Peony disassembly..." -ForegroundColor Yellow
$sw = [System.Diagnostics.Stopwatch]::StartNew()

$output = dotnet run --project src/Peony.Cli -c Release -- `
	disasm $RomPath `
	-f poppy `
	-y $PansyPath `
	-c $CdlPath `
	-o $OutputPath `
	-b 2>&1

$sw.Stop()
$elapsed = $sw.Elapsed

Write-Host "Completed in $($elapsed.TotalSeconds.ToString('F1'))s" -ForegroundColor Green
Write-Host ""

# Parse metrics from output
$totalBlocks = 0
$codeBytes = 0
$bankBlocks = @{}

foreach ($line in $output) {
	$str = $line.ToString()
	if ($str -match "Disassembled (\d+) blocks") {
		$totalBlocks = [int]$Matches[1]
	}
	if ($str -match "Coverage: code (\d[\d,]*),") {
		$codeBytes = [int]($Matches[1] -replace ",", "")
	}
	if ($str -match "Bank\s+(\d+):\s+(\d+)\s+blocks") {
		$bankBlocks[[int]$Matches[1]] = [int]$Matches[2]
	}
}

# Count instructions and .db lines in output
$instrCount = (Select-String -Path $OutputPath -Pattern '^\t[a-z]{2,4}\s' | Where-Object {
	$_.Line -notmatch '^\t\.d[bwl]' -and $_.Line -notmatch '^\t\.(org|bank|base)'
} | Measure-Object).Count

$dbCount = (Select-String -Path $OutputPath -Pattern '^\t\.db\s' | Measure-Object).Count

# Display results
Write-Host "=== Results ===" -ForegroundColor Cyan
Write-Host "  Total blocks:  $totalBlocks"
Write-Host "  Code bytes:    $codeBytes"
Write-Host "  Instructions:  $instrCount"
Write-Host "  .db lines:     $dbCount"
Write-Host "  Banks with code:"
foreach ($b in ($bankBlocks.Keys | Sort-Object)) {
	Write-Host "    Bank $($b.ToString().PadLeft(2)): $($bankBlocks[$b]) blocks"
}
Write-Host ""

# Minimum thresholds (based on post-fix baseline: 2026-04-20)
# These should only go UP as we improve the disassembler.
$minBlocks = 4000
$minCodeBytes = 55000
$minInstructions = 11000
$minBank0Blocks = 1500
$minBanksWithCode = 7

$failures = @()

if ($totalBlocks -lt $minBlocks) {
	$failures += "Total blocks ($totalBlocks) below minimum ($minBlocks)"
}
if ($codeBytes -lt $minCodeBytes) {
	$failures += "Code bytes ($codeBytes) below minimum ($minCodeBytes)"
}
if ($instrCount -lt $minInstructions) {
	$failures += "Instructions ($instrCount) below minimum ($minInstructions)"
}
if (-not $bankBlocks.ContainsKey(0) -or $bankBlocks[0] -lt $minBank0Blocks) {
	$bank0 = if ($bankBlocks.ContainsKey(0)) { $bankBlocks[0] } else { 0 }
	$failures += "Bank 0 blocks ($bank0) below minimum ($minBank0Blocks)"
}
if ($bankBlocks.Count -lt $minBanksWithCode) {
	$failures += "Banks with code ($($bankBlocks.Count)) below minimum ($minBanksWithCode)"
}

if ($failures.Count -gt 0) {
	Write-Host "=== REGRESSION DETECTED ===" -ForegroundColor Red
	foreach ($f in $failures) {
		Write-Host "  FAIL: $f" -ForegroundColor Red
	}
	exit 1
} else {
	Write-Host "=== ALL CHECKS PASSED ===" -ForegroundColor Green
	exit 0
}
