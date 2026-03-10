# Docs Sync Tool (Pull & Push)
# File: G:\Copilot_OutPut\FishingGame\Scripts\SyncDocs.ps1

$rootDir = "G:\Copilot_OutPut\FishingGame"
Set-Location $rootDir

Write-Host ">>> Starting Docs Synchronization..." -ForegroundColor Cyan

# 1. Check Git Status
Write-Host ">>> Checking Git status..." -ForegroundColor Gray
$status = git status --porcelain
if ($status) {
    Write-Host "    Changes detected. Staging files..." -ForegroundColor Yellow
    git add .
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    git commit -m "Docs Update: $timestamp (Auto-sync)"
    Write-Host "    Committed changes locally." -ForegroundColor Green
} else {
    Write-Host "    No local changes to commit." -ForegroundColor Green
}

# 2. Pull from Remote (Rebase)
Write-Host ">>> Pulling latest changes from remote (Rebase)..." -ForegroundColor Cyan
try {
    # Capture output to avoid noisy conflict messages if successful
    $pullOutput = git pull --rebase origin main 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Pull failed. Please resolve conflicts manually. Output: $pullOutput"
    } else {
        Write-Host "    Pull successful." -ForegroundColor Green
    }
} catch {
    Write-Error $_
    exit 1
}

# 3. Push to Remote
Write-Host ">>> Pushing to remote repository..." -ForegroundColor Cyan
try {
    $pushOutput = git push origin main 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Push failed. Please check your network or authentication. Output: $pushOutput"
    } else {
        Write-Host "    Push successful! Your docs are live at https://wangxun111.github.io/qhphysics-docs/" -ForegroundColor Green
    }
} catch {
    Write-Error $_
    Write-Host "    Make sure you have authenticated with GitHub." -ForegroundColor Yellow
    exit 1
}

Read-Host "Press Enter to exit..."

