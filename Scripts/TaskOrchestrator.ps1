# Tool: Task Orchestrator (Auto-Pilot)
# Role: Monitors file changes and dispatches tasks to other agents.
# Usage: .\TaskOrchestrator.ps1 (Run in a separate PowerShell window)
$ErrorActionPreference = "Stop"
$scriptDir = Split-Path $MyInvocation.MyCommand.Path
$configPath = Join-Path $scriptDir "ProjectIntent.json"
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   🤖 TASK ORCHESTRATOR STARTED" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
# 1. Load Configuration
if (-not (Test-Path $configPath)) {
    Write-Error "Config file not found: $configPath"
    exit 1
}
$config = Get-Content $configPath -Raw | ConvertFrom-Json
$scriptsDir = $config.paths.scripts
$kbDir = $config.paths.knowledge_base
$logDir = $config.paths.logs
Write-Host "Monitoring:"
Write-Host "   Docs: $kbDir" -ForegroundColor Gray
Write-Host "   Logs: $logDir" -ForegroundColor Gray
# 2. Define Actions
$actionDocUpdate = {
    param($Event)
    $path = $Event.SourceEventArgs.FullPath
    $name = $Event.SourceEventArgs.Name
    $changeType = $Event.SourceEventArgs.ChangeType
    Write-Host "`n[$((Get-Date).ToString("HF:mm:ss"))] 📝 Detected $changeType in $name" -ForegroundColor Yellow
    # Reload config to check enabled status dynamically
    $localConfig = Get-Content $Global:configPath -Raw | ConvertFrom-Json
    if ($localConfig.agents_config.DocBuilder.enabled) {
        $builder = Join-Path $Global:scriptsDir $localConfig.agents_config.DocBuilder.script
        if (Test-Path $builder) {
            Write-Host "   >>> 1. Building HTML..." -ForegroundColor DarkGray
            Start-Process "powershell" -ArgumentList "-ExecutionPolicy Bypass -File `"$builder`" -TargetFile `"$path`"" -NoNewWindow -Wait
        }
    }
    if ($localConfig.agents_config.Inspector.enabled) {
         # HTML Path
         $htmlPath = $path -replace "\.md$", ".html"
         if (Test-Path $htmlPath) {
             $inspector = Join-Path $Global:scriptsDir $localConfig.agents_config.Inspector.script
             Write-Host "   >>> 2. Inspecting..." -ForegroundColor DarkGray
             Start-Process "powershell" -ArgumentList "-ExecutionPolicy Bypass -File `"$inspector`" -FilePath `"$htmlPath`"" -NoNewWindow -Wait
         }
    }
    if ($localConfig.agents_config.Navigator.enabled) {
        $nav = Join-Path $Global:scriptsDir $localConfig.agents_config.Navigator.script
        Write-Host "   >>> 3. Updating Index..." -ForegroundColor DarkGray
        Start-Process "powershell" -ArgumentList "-ExecutionPolicy Bypass -File `"$nav`"" -NoNewWindow -Wait
    }
    Write-Host "   ✅ Task Complete." -ForegroundColor Green
}
$actionLogDetect = {
    param($Event)
    $path = $Event.SourceEventArgs.FullPath
    Write-Host "`n[$((Get-Date).ToString("HH:mm:ss"))] 🕵️ New Log Detected: $(Split-Path $path -Leaf)" -ForegroundColor Magenta
    $localConfig = Get-Content $Global:configPath -Raw | ConvertFrom-Json
    if ($localConfig.agents_config.LogDetective.enabled) {
        $script = Join-Path $Global:scriptsDir $localConfig.agents_config.LogDetective.script # e.g. AnalyzeJitter.ps1
        # We assume the analysis script takes the log path as argument
        # Check if the script exists first (AnalyzeJitter might be missing)
        if (Test-Path $script) {
             Write-Host "   >>> Analyzing Data..." -ForegroundColor DarkGray
             Start-Process "powershell" -ArgumentList "-ExecutionPolicy Bypass -File `"$script`" -LogFile `"$path`"" -NoNewWindow
        } else {
             Write-Warning "   >>> Analysis Script not found: $script"
        }
    }
}
# 3. Setup FileSystemWatchers
# A. Knowledge Base Watcher
$kbWatcher = New-Object System.IO.FileSystemWatcher
$kbWatcher.Path = $kbDir
$kbWatcher.Filter = "*.md"
$kbWatcher.IncludeSubdirectories = $true
$kbWatcher.EnableRaisingEvents = $true
# Register Event
$Global:configPath = $configPath
$Global:scriptsDir = $scriptsDir
Register-ObjectEvent $kbWatcher "Changed" -SourceIdentifier "KBFileChanged" -Action $actionDocUpdate
Register-ObjectEvent $kbWatcher "Created" -SourceIdentifier "KBFileCreated" -Action $actionDocUpdate
# (Renamed/Deleted handling omitted for simplicity, usually triggers Changed or Created on save)
# B. Log Watcher
if (Test-Path $logDir) {
    $logWatcher = New-Object System.IO.FileSystemWatcher
    $logWatcher.Path = $logDir
    $logWatcher.Filter = $config.agents_config.LogDetective.pattern
    $logWatcher.IncludeSubdirectories = $false
    $logWatcher.EnableRaisingEvents = $true
    Register-ObjectEvent $logWatcher "Created" -SourceIdentifier "LogFileCreated" -Action $actionLogDetect
}
Write-Host "`n✅ Orchestrator is listening... (Press Ctrl+C to stop)" -ForegroundColor Green
# Keep script running
while ($true) {
    Start-Sleep -Seconds 5
}
