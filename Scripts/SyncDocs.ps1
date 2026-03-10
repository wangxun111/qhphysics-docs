# 文档同步工具 (拉取 & 推送)
# 文件: G:\Copilot_OutPut\FishingGame\Scripts\SyncDocs.ps1
$rootDir = "G:\Copilot_OutPut\FishingGame"
Set-Location $rootDir
Write-Host ">>> 开始文档同步..." -ForegroundColor Cyan
# 0. 保护现场 (Stash)
$stashOutput = git stash push -u -m "AutoSync_Temp_Stash" 2>&1
$hasStashed = $stashOutput -match "Saved working directory"
# 1. 从远程拉取 (变基)
Write-Host ">>> 正在从远程拉取最新更改 (Rebase)..." -ForegroundColor Cyan
git pull --rebase origin main 2>&1 | Write-Host -ForegroundColor Cyan
if ($LASTEXITCODE -ne 0) {
    Write-Host "!!! 拉取失败 !!!" -ForegroundColor Red
    Write-Host "错误信息：" -ForegroundColor Yellow
    # 再次尝试一次 fetch 以获取具体的错误并显示
    git fetch origin main 2>&1 | Write-Host -ForegroundColor Gray
    # 询问用户是否强制推送
    $choice = Read-Host "可能是因为网络问题或冲突。是否忽略拉取错误并尝试强制推送��(Y/N)"
    if ($choice -ne 'Y') { 
        if ($hasStashed) { git stash pop | Out-Null }
        exit 1 
    }
} else {
    Write-Host "    拉取成功。" -ForegroundColor Green
}
# 恢复现场
if ($hasStashed) {
    Write-Host ">>> 正在恢复本地更改..." -ForegroundColor Gray
    git stash pop | Out-Null
}
# 2. 检查 Git 状态并提交
Write-Host ">>> 正在检查 Git 状态..." -ForegroundColor Gray
$status = git status --porcelain
if ($status) {
    Write-Host "    检测到更改。正在暂存文件..." -ForegroundColor Yellow
    git add .
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    git commit -m "文档更新: $timestamp (自动同步)"
    Write-Host "    本地更改已提交。" -ForegroundColor Green
} else {
    Write-Host "    没有需要提交的本地更改。" -ForegroundColor Green
}
# 3. 推送到远程
Write-Host ">>> 正在推送到远程仓库..." -ForegroundColor Cyan
$retryCount = 0
$maxRetries = 3
$success = $false
do {
    git push origin main
    if ($LASTEXITCODE -eq 0) {
        $success = $true
        Write-Host "    推送成功！您的文档已上线: https://wangxun111.github.io/qhphysics-docs/" -ForegroundColor Green
    } else {
        $retryCount++
        if ($retryCount -lt $maxRetries) {
            Write-Host "!!! 推送失败，正在重试 ($retryCount/$maxRetries)..." -ForegroundColor Yellow
            Start-Sleep -Seconds 2
        } else {
             Write-Host "推送最终失败。请检查网络连接。" -ForegroundColor Red
             git push origin main 2>&1 | Write-Host -ForegroundColor Red
        }
    }
} until ($success -or $retryCount -ge $maxRetries)
Read-Host "按回车键退出..."
