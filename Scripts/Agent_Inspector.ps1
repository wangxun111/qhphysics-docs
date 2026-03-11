# Agent 3: 质量检察员 (Inspector)
# 职责: 检查产物文件的完整性、错误标记
# 文件: G:\Copilot_OutPut\FishingGame\Scripts\Agent_Inspector.ps1

param(
    [string]$FilePath
)

# 0. 基础环境
$scriptPath = $MyInvocation.MyCommand.Path
if (-not $scriptPath) {
    # 如果作为脚本块运行，$MyInvocation 可能为空，尝试从 PWD 或参数推断
    # 假设 InspectorRules.json 在同一目录下
    $scriptDir = $PSScriptRoot
    if (-not $scriptDir) { $scriptDir = "G:\Copilot_OutPut\FishingGame\Scripts" } 
} else {
    $scriptDir = Split-Path $scriptPath
}
$rulesPath = Join-Path $scriptDir "InspectorRules.json"

# 1. 基础检查
if (-not (Test-Path $FilePath)) {
    Write-Host "    [Inspector] ❌ FAIL: File not found: $FilePath" -ForegroundColor Red
    exit 1
}

$fileItem = Get-Item $FilePath
$fileName = $fileItem.Name

# 2. 大小检查
if ($fileItem.Length -lt 100) {
    Write-Host "    [Inspector] ❌ FAIL: File too small ($($fileItem.Length) bytes): $fileName" -ForegroundColor Red
    exit 1
}

# 3. 读取内容 (Read content once)
try {
    $content = Get-Content $FilePath -Raw -Encoding UTF8
} catch {
    Write-Host "    [Inspector] ❌ FAIL: Cannot read file content: $_" -ForegroundColor Red
    exit 1
}

# 4. 动态规则检查 (Dynamic Rule Check)
if (Test-Path $rulesPath) {
    try {
        $rulesJson = Get-Content $rulesPath -Raw -Encoding UTF8
        $rules = $rulesJson | ConvertFrom-Json
        
        foreach ($rule in $rules) {
            # 简单正则匹配
            if ($content -match $rule.pattern) {
                # 某些规则可能针对特定文件类型
                if ($rule.category -eq "Code" -and $fileName -notmatch "\.cs$") { continue }
                if ($rule.category -eq "HTML/Mermaid" -and $fileName -notmatch "\.(html|md)$") { continue }

                # 发现违规
                Write-Host "    [Inspector] ❌ FAIL [Rule $($rule.id)]: $($rule.description)" -ForegroundColor Red
                Write-Host "    [Suggestion]: $($rule.fix_suggestion)" -ForegroundColor Yellow
                exit 1
            }
        }
    } catch {
        Write-Warning "    [Inspector] ⚠️ Failed to load rules from $rulesPath : $_"
    }
} else {
    Write-Warning "    [Inspector] ⚠️ No rules file found at $rulesPath"
}

# 5. 硬编码检查 (针对 HTML)
if ($fileName.EndsWith(".html")) {
    # 检查是否为空 MD 转换结果
    if ($content -match 'const md = ""') {
         Write-Host "    [Inspector] ❌ FAIL: Empty Markdown content detected." -ForegroundColor Red
         exit 1
    }
}

Write-Host "    [Inspector] ✅ PASS: Quality check passed ($fileName)" -ForegroundColor Green
exit 0
