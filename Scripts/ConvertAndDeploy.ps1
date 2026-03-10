# GitHub Pages Deployment Automator
# File: G:\Copilot_OutPut\FishingGame\Scripts\ConvertAndDeploy.ps1
$rootDir = "G:\Copilot_OutPut\FishingGame"
$kbDir = "$rootDir\KnowledgeBase"
$htmlDir = "$rootDir\html"
$scriptsDir = "$rootDir\Scripts"
$converterScript = "$scriptsDir\ConvertMdToHtml.ps1"
# 1. Prepare: Sync Key Docs
Write-Host ">>> Syncing Key Project Docs..." -ForegroundColor Cyan
$projectRulesSource = "F:\new\fishinggame\AI_PROJECT_RULES.md"
if (Test-Path $projectRulesSource) {
    Copy-Item $projectRulesSource -Destination "$kbDir\AI_PROJECT_RULES.md" -Force
    Write-Host "    AI_PROJECT_RULES.md copied to KnowledgeBase." -ForegroundColor Gray
}
# 2. Batch Convert Markdown to HTML
Write-Host ">>> Converting Markdowns to HTML..." -ForegroundColor Cyan
if (Test-Path $converterScript) {
    $mdFiles = Get-ChildItem -Path $kbDir -Filter "*.md"
    foreach ($file in $mdFiles) {
        Write-Host "    Processing: " -ForegroundColor Gray
        & $converterScript -InputPath $file.FullName
    }
} else {
    Write-Error "    Converter script not found: $converterScript"
}
# 3. Git Init
Write-Host ">>> Configuring Git Repo..." -ForegroundColor Cyan
Set-Location $rootDir
if (-not (Test-Path ".git")) {
    git init
    Write-Host "    Git init done." -ForegroundColor Green
    git remote add origin "https://github.com/wangxun111/qhphysics-docs.git"
    Write-Host "    Remote origin added." -ForegroundColor Green
    git checkout -b main
} else {
    Write-Host "    Git repo already exists." -ForegroundColor Yellow
}
# 4. Configure .gitignore
$ignoreList = "Log/", "*.tmp", "*.log", ".DS_Store"
Set-Content -Path ".gitignore" -Value $ignoreList
Write-Host "    .gitignore updated." -ForegroundColor Gray
# 5. Commit Changes
Write-Host ">>> Committing Changes..." -ForegroundColor Cyan
git add .
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
git commit -m "Docs update: $timestamp"
# 6. Push Instructions
Write-Host "
>>> Done!" -ForegroundColor Green
Write-Host "Please run the following command manually to push to GitHub:" -ForegroundColor Yellow
Write-Host "    git push -u origin main"
