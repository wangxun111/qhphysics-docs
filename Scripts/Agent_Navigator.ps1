# Agent 2: Navigation Manager
# Role: Scans directory structure, updates site_nav.js
# File: G:\Copilot_OutPut\FishingGame\Scripts\Agent_Navigator.ps1
$ErrorActionPreference = "Stop"
$rootDir = "G:\Copilot_OutPut\FishingGame"
$navDataFile = "$rootDir\site_nav.js"
try {
    $siteStructure = @{}
    $filesFound = 0
    $rootItems = @()
    $kbItems = @()
    $htmlItems = @()
    $reviewItems = @()
    # --- 1. Scan Root Files ---
    if (Test-Path $rootDir) {
        $rootFiles = Get-ChildItem -Path $rootDir -Filter "*.html" | Where-Object { $_.Name -match "START_|QUICK_|TESTING_" }
        foreach ($f in $rootFiles) {
            $rootItems += @{ title = $f.Name; path = $f.Name; type = "html" }
        }
    }
    # --- 2. Scan KnowledgeBase (Exclude PostMortem) ---
    if (Test-Path "$rootDir\KnowledgeBase") {
        # Only scan top-level HTMLs to keep Knowledge Base clean
        $kbFileItems = Get-ChildItem -Path "$rootDir\KnowledgeBase" -Filter "*.html"
        foreach ($f in $kbFileItems) {
            if ($f.Name -eq "index.html") { continue }
            $kbItems += @{ title = $f.BaseName; path = "KnowledgeBase/" + $f.Name; type = "kb" }
        }
    }
    # --- 2.5 Scan PostMortem / Reviews ---
    $postMortemDir = "$rootDir\KnowledgeBase\PostMortem"
    if (Test-Path $postMortemDir) {
        $pmFiles = Get-ChildItem -Path $postMortemDir -Filter "*.html"
        foreach ($f in $pmFiles) {
            $reviewItems += @{ title = $f.BaseName; path = "KnowledgeBase/PostMortem/" + $f.Name; type = "review" }
        }
    }
    # --- 3. Scan HTML (Reports) ---
    if (Test-Path "$rootDir\html") {
        $htmlFileItems = Get-ChildItem -Path "$rootDir\html" -Filter "*.html" | Where-Object { $_.Name -ne "index.html" }
        foreach ($f in $htmlFileItems) {
            $htmlItems += @{ title = $f.Name; path = "html/" + $f.Name; type = "report" }
        }
    }
    # --- 4. Build Structure keys in English ---
    if ($rootItems.Count -gt 0) {
        $siteStructure["0_Root"] = @{ title = "Core Docs"; items = $rootItems }
    }
    if ($kbItems.Count -gt 0) {
        $siteStructure["1_KB"] = @{ title = "Knowledge Base"; items = $kbItems }
    }
    if ($reviewItems.Count -gt 0) {
        $siteStructure["3_Reviews"] = @{ title = "Project Reviews"; items = $reviewItems }
    }
    if ($htmlItems.Count -gt 0) {
        $siteStructure["2_Reports"] = @{ title = "Visual Reports"; items = $htmlItems }
    }
    $totalFound = $rootItems.Count + $kbItems.Count + $htmlItems.Count + $reviewItems.Count
    if ($totalFound -eq 0) {
        Write-Warning "    [Navigator] Found 0 items. Aborting overwrite."
        exit 1
    }
    # --- 5. Write JSON ---
    $jsonContent = $siteStructure | ConvertTo-Json -Depth 5 -Compress
    $jsContent = "window.SITE_NAV = $jsonContent;"
    Set-Content -Path $navDataFile -Value $jsContent -Encoding UTF8
    Write-Host "    [Navigator] Site Map Updated ($totalFound items)" -ForegroundColor Green
} catch {
    Write-Host "    [Navigator] Error: $_" -ForegroundColor Red
}
