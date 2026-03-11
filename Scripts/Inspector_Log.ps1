# Agent 3C: 日志质检员 (Log Inspector)
# 职责: 专注于日志格式和内容分析的准确性
# 文件: G:\Copilot_OutPut\FishingGame\Scripts\Inspector_Log.ps1

param(
    [string]$FilePath
)

# 0. 基础环境
$scriptPath = $MyInvocation.MyCommand.Path
if (-not $scriptPath) {
    if ($PSScriptRoot) { $scriptDir = $PSScriptRoot } else { $scriptDir = "G:\Copilot_OutPut\FishingGame\Scripts" }
} else {
    $scriptDir = Split-Path $scriptPath
}
$rulesPath = Join-Path $scriptDir "InspectorRules.json"

# 1. 基础检查
if (-not (Test-Path $FilePath) -or -not ($FilePath.EndsWith(".txt") -or $FilePath.EndsWith(".log"))) {
    Write-Host "    [LogInspector] ❌ FAIL: Invalid file type: $FilePath" -ForegroundColor Red
    exit 1
}

$fileItem = Get-Item $FilePath
$fileName = $fileItem.Name

# 2. 读取内容
try {
    $content = Get-Content $FilePath -Raw -Encoding UTF8
} catch {
    Write-Host "    [LogInspector] ❌ FAIL: Cannot read log file: $_" -ForegroundColor Red
    exit 1
}

# 3. 动态规则检查 (仅加载 Category=Log 的规则)
if (Test-Path $rulesPath) {
    try {
        $rules = Get-Content $rulesPath -Raw -Encoding UTF8 | ConvertFrom-Json
        foreach ($rule in $rules) {
            # 过滤只属于日志的规则
            if ($rule.category -and $rule.category -ne "Log") { continue }

            if ($content -match $rule.pattern) {
                Write-Host "    [LogInspector] ❌ FAIL [Rule $($rule.id)]: $($rule.description)" -ForegroundColor Red
                Write-Host "    [Suggestion]: $($rule.fix_suggestion)" -ForegroundColor Yellow
                exit 1
            }
        }
    } catch {
        Write-Warning "    [LogInspector] ⚠️ Failed to load rules: $_"
    }
}

# 4. 关键指标校验
if ($content -match "EXCEPTION") {
     Write-Host "    [LogInspector] ❌ FAIL: Critical Exception keyword found in log." -ForegroundColor Red
     exit 1
}

Write-Host "    [LogInspector] ✅ PASS: Log Analysis Format ($fileName)" -ForegroundColor Green
exit 0
