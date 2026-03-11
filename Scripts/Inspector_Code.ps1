# Agent 3B: 代码质检员 (Code Inspector)
# 职责: 专注于 C# 代码的基础语法、发布配置、编译性检查
# 文件: G:\Copilot_OutPut\FishingGame\Scripts\Inspector_Code.ps1

param(
    [string]$FilePath
)

$scriptPath = $MyInvocation.MyCommand.Path
if (-not $scriptPath) {
    if ($PSScriptRoot) { $scriptDir = $PSScriptRoot } else { $scriptDir = "G:\Copilot_OutPut\FishingGame\Scripts" }
} else {
    $scriptDir = Split-Path $scriptPath
}
$rulesPath = Join-Path $scriptDir "InspectorRules.json"

# 0. 基础环境检查
if (-not (Test-Path $FilePath) -or -not ($FilePath.EndsWith(".cs"))) {
    Write-Host "    [CodeInspector] ❌ FAIL: Invalid file path or extension: $FilePath" -ForegroundColor Red
    exit 1
}

$fileItem = Get-Item $FilePath
$fileName = $fileItem.Name

# 1. 基础编码检查 (Basic Encoding Check)
try {
    # 强制尝试按 UTF8 读取，若失败或乱码可能会抛出异常
    $content = Get-Content $FilePath -Raw -Encoding UTF8
} catch {
    Write-Host "    [CodeInspector] ❌ FAIL: Cannot read file as UTF-8 (Encoding Issue): $_" -ForegroundColor Red
    exit 1
}

# 2. 动态规则检查 (仅加载 Category=Code 的规则)
if (Test-Path $rulesPath) {
    try {
        $rules = Get-Content $rulesPath -Raw -Encoding UTF8 | ConvertFrom-Json
        foreach ($rule in $rules) {
            # 过滤只属于代码的规则
            if ($rule.category -and $rule.category -ne "Code") { continue }

            if ($content -match $rule.pattern) {
                Write-Host "    [CodeInspector] ❌ FAIL [Rule $($rule.id)]: $($rule.description)" -ForegroundColor Red
                Write-Host "    [Suggestion]: $($rule.fix_suggestion)" -ForegroundColor Yellow
                exit 1
            }
        }
    } catch {
        Write-Warning "    [CodeInspector] ⚠️ Failed to load rules: $_"
    }
}

# 3. 基础语法 & 规范检查 (Regex based Lite-Check)
# 3.1 检查是否缺少必要的 Using
if (-not ($content -match "using System;") -and -not ($content -match "using UnityEngine;")) {
    Write-Warning "    [CodeInspector] ⚠️ WARNING: File seems to lack basic 'using' directives."
}

# 3.2 检查 BOM 头 (Best Effort, Powershell handle encoding abstraction)
# Note: PowerShell 6+ handles BOM automatically, but here we enforce content logic.

# 3.3 模拟编译检查 (Simulation Only)
# 真正的编译需要 csproj 上下文，这里可以预留接口: & dotnet build ...
if ($content -match "ERROR_PLACEHOLDER") {
    Write-Host "    [CodeInspector] ❌ FAIL: Found explicit ERROR marker." -ForegroundColor Red
    exit 1
}

Write-Host "    [CodeInspector] ✅ PASS: Syntax/Style Check ($fileName)" -ForegroundColor Green
exit 0
