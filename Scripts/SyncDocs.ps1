# 文档同步工具 (拉取 & 推送)
# 文件: G:\Copilot_OutPut\FishingGame\Scripts\SyncDocs.ps1
$rootDir = "G:\Copilot_OutPut\FishingGame"
Set-Location $rootDir
Write-Host ">>> 开始文档同步..." -ForegroundColor Cyan
# 1. 检查 Git 状态
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
# 2. 从远程拉取 (变基)
Write-Host ">>> 正在从远程拉取最新更改 (Rebase)..." -ForegroundColor Cyan
git pull --rebase origin main
if ($LASTEXITCODE -ne 0) {
    Write-Error "拉取失败。请手动解决冲突。"
    exit 1
} else {
    Write-Host "    拉取成功。" -ForegroundColor Green
}
# 3. 推送到远程
Write-Host ">>> 正在推送到远程仓库..." -ForegroundColor Cyan
git push origin main
if ($LASTEXITCODE -ne 0) {
    Write-Error "推送失败。请检查网络或身份验证。"
    Write-Host "    请确保您已通过 GitHub 身份验证 (例如使用 'gh auth login' 或凭据管理器)。" -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "    推送成功！您的文档已上线: https://wangxun111.github.io/qhphysics-docs/" -ForegroundColor Green
}
Read-Host "按回车键退出..."
