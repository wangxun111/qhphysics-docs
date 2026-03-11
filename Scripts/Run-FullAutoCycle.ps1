# Tool: Full Auto Cycle (Run-FullAutoCycle.ps1)
# Role: Orchestrates the entire documentation build pipeline in one go.
# Usage: .\Run-FullAutoCycle.ps1
$ErrorActionPreference = "Stop"
$rootDir = "G:\Copilot_OutPut\FishingGame"
$scriptsDir = Join-Path $rootDir "Scripts"
$siteNav = Join-Path $rootDir "site_nav.js"
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   🚀 STARTING FULL AUTO CYCLE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
# Step 1: Pre-Flight Syntax Check
$syntaxChecker = Join-Path $scriptsDir "Check-ScriptSyntax.ps1"
if (Test-Path $syntaxChecker) {
    Write-Host "`n[1/4] Pre-checking Script Syntax..." -ForegroundColor Yellow
    $coreScripts = @("Agent_DocBuilder.ps1", "Agent_Navigator.ps1", "Inspector_Doc.ps1")
    foreach ($fn in $coreScripts) {
        $sp = Join-Path $scriptsDir $fn
        if (Test-Path $sp) {
            & $syntaxChecker -Path $sp
            if ($LASTEXITCODE -ne 0) {
                Write-Error "CRITICAL: Script '$fn' has syntax errors. Aborting."
                exit 1
            }
        }
    }
}
# Step 2: Build Documentation (HTML Generation)
$docBuilder = Join-Path $scriptsDir "Agent_DocBuilder.ps1"
if (Test-Path $docBuilder) {
    Write-Host "`n[2/4] Building Documentation..." -ForegroundColor Yellow
    & $docBuilder
} else {
    Write-Error "DocBuilder script missing!"
}
# Step 3: Update Navigation Index
$navigator = Join-Path $scriptsDir "Agent_Navigator.ps1"
if (Test-Path $navigator) {
    Write-Host "`n[3/4] Updating Navigation Index..." -ForegroundColor Yellow
    & $navigator
} else {
    Write-Error "Navigator script missing!"
}
# Step 4: Final Quality Inspection
$inspector = Join-Path $scriptsDir "Inspector_Doc.ps1"
if (Test-Path $inspector) {
    Write-Host "`n[4/4] Performing Final Quality Check..." -ForegroundColor Yellow
    # Check site_nav.js
    & $inspector -FilePath $siteNav
    if ($LASTEXITCODE -ne 0) {
        Write-Error "CRITICAL: site_nav.js failed inspection."
        exit 1
    }
    # Check Portal (index.html)
    & $inspector -FilePath (Join-Path $rootDir "index.html")
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "WARNING: Portal index.html might have issues."
    }
}
Write-Host "`n✅ CYCLE COMPLETE SUCCESSFULLY!" -ForegroundColor Green
Write-Host "   View at: file:///$rootDir/index.html" -ForegroundColor Gray
