# index.html 生成器
# 文件: G:\Copilot_OutPut\FishingGame\Scripts\GenerateSiteData.ps1

$rootDir = "G:\Copilot_OutPut\FishingGame"
$outputFile = "$rootDir\site_data.js"

# 定义要扫描的目录及其显示名称
$targetFolders = @{
    "KnowledgeBase" = "📚 知识库 (Markdown)";
    "html"         = "📊 可视化报表 (HTML)";
    "Scripts"      = "🛠️ 工具脚本 (Scripts)";
}

# 开始构建 JSON
$jsonParts = @()

foreach ($folderName in $targetFolders.Keys) {
    $displayName = $targetFolders[$folderName]
    $folderPath = Join-Path $rootDir $folderName
    
    if (Test-Path $folderPath) {
        $files = Get-ChildItem -Path $folderPath -Recurse | Where-Object { ! $_.PSIsContainer -and $_.Name -ne "index.html" }
        
        $fileObjects = @()
        foreach ($file in $files) {
            # 获取相对路径 (例如 KnowledgeBase\Doc.md)
            $relativePath = $file.FullName.Substring($rootDir.Length + 1).Replace("\", "/")
            $ext = $file.Extension.ToLower().TrimStart('.')
            
            # 构建简单的 JS 对象字符串
            $fileObjects += "{ title: '$($file.Name)', path: '$relativePath', type: '$ext', size: '$([math]::Round($file.Length/1KB, 2)) KB' }"
        }
        
        if ($fileObjects.Count -gt 0) {
            $filesJson = $fileObjects -join ","
            $jsonParts += "'$folderName': { displayName: '$displayName', files: [$filesJson] }"
        }
    }
}

# 组合最终的 JS 内容
$jsContent = "const siteData = { " + ($jsonParts -join ", ") + " };"

# 写入文件
Set-Content -Path $outputFile -Value $jsContent -Encoding UTF8
Write-Host ">>> site_data.js 已生成 (包含 $($jsonParts.Count) 个文件夹分类)" -ForegroundColor Green

