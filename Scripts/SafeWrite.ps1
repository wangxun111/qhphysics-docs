# 工具: 安全写入 (SafeWrite)
# 职责: 在覆盖关键文件前，先写入临时文件并经过质检，通过后才覆盖
# 用法: .\SafeWrite.ps1 -Path "Target.md" -Value "Content" -Validator "Inspector_Doc"
param(
    [Parameter(Mandatory=$true)]
    [string]$Path,
    [Parameter(Mandatory=$true)]
    [string]$Value,
    [string]$Validator = ""
)
$ErrorActionPreference = "Stop"
$scriptDir = $PSScriptRoot
Write-Host "    [SafeWrite] ?? Initiating safe write to: $Path" -ForegroundColor Gray
try {
    # 1. 准备临时路径
    $tempPath = "$Path.tmp"
    # 2. 写入临时文件
    Set-Content -Path $tempPath -Value $Value -Encoding UTF8 -Force
    # 3. 验证 (如果有指定验证器)
    if (-not [string]::IsNullOrWhiteSpace($Validator)) {
        # 假设 Validator 是同一个目录下的脚本名
        $validatorScript = Join-Path $scriptDir "$Validator.ps1"
        if (Test-Path $validatorScript) {
            Write-Host "    [SafeWrite] ?? Running validator: $Validator" -ForegroundColor DarkGray
            # 运行验证脚本
            & $validatorScript -FilePath $tempPath
            if ($LASTEXITCODE -ne 0) {
                Throw "Validator '$Validator' failed (ExitCode: $LASTEXITCODE). Write aborted."
            }
        } else {
            Write-Warning "    [SafeWrite] ?? Validator script not found: $validatorScript"
        }
    } else {
        # 默认验证: 文件大小
        $item = Get-Item $tempPath
        if ($item.Length -eq 0) {
            Throw "Zero-byte file generated. Write aborted."
        }
    }
    # 4. 覆盖原文件 (Atomic Move)
    if (Test-Path $Path) {
        Remove-Item $Path -Force
    }
    Move-Item $tempPath $Path -Force
    Write-Host "    [SafeWrite] ? Write committed successfully." -ForegroundColor Green
} catch {
    Write-Host "    [SafeWrite] ?? Write Failed: $_" -ForegroundColor Red
    if (Test-Path $tempPath) { Remove-Item $tempPath -Force }
    exit 1
}
