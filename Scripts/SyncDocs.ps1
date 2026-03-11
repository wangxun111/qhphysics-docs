# 文档同步工具 (Docsify 版)
# 文件: G:\Copilot_OutPut\FishingGame\Scripts\SyncDocs.ps1
$rootDir = "G:\Copilot_OutPut\FishingGame"
Set-Location $rootDir
Write-Host ">>> Docsify Sync Tool Started..." -ForegroundColor Cyan

# 1. 自动生成侧边栏
Write-Host ">>> Generating _sidebar.md..." -ForegroundColor Cyan
& "$rootDir\Scripts\GenerateSidebar.ps1"

# 2. 确保 README.md 存在 (用于首页)
if (-not (Test-Path "README.md")) {
    if (Test-Path "START_HERE.md") {
        Copy-Item "START_HERE.md" "README.md"
        Write-Host ">>> Copied START_HERE.md to README.md as homepage." -ForegroundColor Yellow
    }
}

# 3. 保护现场 (Stash)
$stashOutput = git stash push -u -m "Docsify_AutoSync" 2>&1
$hasStashed = $stashOutput -match "Saved working directory"

# 4. 从远程拉取
Write-Host ">>> Pulling from remote..." -ForegroundColor Cyan
git pull --rebase origin main 2>&1 | Write-Host -ForegroundColor Cyan
if ($LASTEXITCODE -ne 0) {
    Write-Host "!!! Pull Failed !!!" -ForegroundColor Red
    if ($hasStashed) { git stash pop | Out-Null }
    $choice = Read-Host "Ignore pull errors and push anyway? (Y/N)"
    if ($choice -ne 'Y') { exit 1 }
}

# 5. 恢复现场
if ($hasStashed) {
    Write-Host ">>> Restoring local changes..." -ForegroundColor Gray
    git stash pop | Out-Null
}

# 6. 提交更改
Write-Host ">>> Committing changes..." -ForegroundColor Cyan
$status = git status --porcelain
if ($status) {
    git add .
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    git commit -m "Docs Update: $timestamp (Docsify Refactor)"
}

# 7. 推送
Write-Host ">>> Pushing to GitHub..." -ForegroundColor Cyan
$retryCount = 0
$maxRetries = 3
$success = $false

do {
    git push origin main
    if ($LASTEXITCODE -eq 0) {
        $success = $true
        Write-Host ">>> SUCCESS! Site is live at: https://wangxun111.github.io/qhphysics-docs/" -ForegroundColor Green
    } else {
        $retryCount++
        if ($retryCount -lt $maxRetries) {
            Write-Host "!!! Push Failed. Retrying ($retryCount/$maxRetries)..." -ForegroundColor Yellow
            Start-Sleep -Seconds 3
        } else {
            Write-Host "!!! Push Failed !!! Check network/auth." -ForegroundColor Red
            git push origin main 2>&1 
        }
    }
} until ($success -or $retryCount -ge $maxRetries)

Read-Host "Press Enter to exit..."
