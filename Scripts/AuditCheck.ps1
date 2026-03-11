$logFile = "G:\Copilot_OutPut\FishingGame\AuditLog.txt"
"Starting Audit..." | Out-File $logFile

$mdFile = "G:\Copilot_OutPut\FishingGame\KnowledgeBase\MultiAgent_Overview_Audit_Report.md"
if (-not (Test-Path $mdFile)) {
    "Creating MD file..." | Out-File $logFile -Append
    New-Item -Path $mdFile -Value "# 🔍 专项审计报告: MultiAgent 架构图`n`n**审计时间**: $(Get-Date)" -Force | Out-Null
}

"Running BuildSite.ps1..." | Out-File $logFile -Append
& "G:\Copilot_OutPut\FishingGame\Scripts\BuildSite.ps1" 2>&1 | Out-File $logFile -Append

$htmlFile = "G:\Copilot_OutPut\FishingGame\html\MultiAgent_Overview_Audit_Report.html"
if (Test-Path $htmlFile) {
    "HTML File Exists: $htmlFile" | Out-File $logFile -Append
} else {
    "HTML File MISSING: $htmlFile" | Out-File $logFile -Append
    # Try alternate location
    $altHtml = "G:\Copilot_OutPut\FishingGame\KnowledgeBase\MultiAgent_Overview_Audit_Report.html"
    if (Test-Path $altHtml) {
        "HTML Found in KB: $altHtml" | Out-File $logFile -Append
    }
}

$navFile = "G:\Copilot_OutPut\FishingGame\site_nav.js"
if (Select-String -Path $navFile -Pattern "MultiAgent_Overview_Audit_Report") {
    "Found in Navigator Index" | Out-File $logFile -Append
} else {
    "MISSING from Navigator Index" | Out-File $logFile -Append
}

"Audit Complete." | Out-File $logFile -Append
