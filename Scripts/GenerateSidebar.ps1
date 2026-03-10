# Docsify 侧边栏生成器
# 文件: G:\Copilot_OutPut\FishingGame\Scripts\GenerateSidebar.ps1

$rootDir = "G:\Copilot_OutPut\FishingGame"
$outDir = $rootDir # Sidebar 应在根目录
$outFile = "$outDir\_sidebar.md"

# 初始化 Sidebar 内容
$sidebarContent = "*   [🏠 首页 (Home)](/README.md)`n"

# 1. 重要根文件
$rootFiles = @("START_HERE.md", "QUICK_REFERENCE.md", "TESTING_GUIDE.md")
foreach ($f in $rootFiles) {
    if (Test-Path "$rootDir\$f") {
        $sidebarContent += "*   [📄 $f](/$f)`n"
    }
}

# 2. 文件夹遍历 (KnowledgeBase, html)
$targetFolders = @("KnowledgeBase", "Scripts")

foreach ($folder in $targetFolders) {
    if (Test-Path "$rootDir\$folder") {
        $sidebarContent += "`n*   **📁 $folder**`n"
        
        $files = Get-ChildItem -Path "$rootDir\$folder" | Sort-Object Name
        foreach ($file in $files) {
            # 处理 .md 文件
            if ($file.Extension -eq ".md") {
                # Docsify 链接无需 .md 后缀，但加上也无妨
                $name = $file.Name.Replace(".md", "").Replace("_", " ")
                $path = "$folder/$($file.Name)"
                $sidebarContent += "    *   [$name](/$path)`n"
            }
            # 处理 .html 报告 (作为外部链接打开，或者 iframe 嵌入)
            # Docsify 默认处理 MD，HTML 文件通过链接跳转
            elseif ($file.Extension -eq ".html") {
                 # 跳过 index.html
                 if ($file.Name -eq "index.html") { continue }
                 $name = $file.Name
                 $path = "$folder/$($file.Name)"
                 $sidebarContent += "    *   [📊 $name](/$path ':ignore')`n"
            }
             # 脚本
            elseif ($file.Extension -match "ps1|py|bat") {
                 $name = $file.Name
                 $path = "$folder/$($file.Name)"
                 # 脚本文件当作代码展示，或者直接下载
                 $sidebarContent += "    *   [💻 $name](/$path ':ignore')`n"
            }
        }
    }
}

# 3. 处理 HTML 文件夹
# HTML 文件夹特殊：包含大量我们生成的交互式图表
if (Test-Path "$rootDir\html") {
    $sidebarContent += "`n*   **📊 Visualization (HTML)**`n"
    $htmlFiles = Get-ChildItem -Path "$rootDir\html" -Filter "*.html" | Sort-Object Name
    foreach ($file in $htmlFiles) {
        $sidebarContent += "    *   [$( $file.Name )](/html/$( $file.Name ) ':ignore')`n"
    }
}

Set-Content -Path $outFile -Value $sidebarContent -Encoding UTF8
Write-Host "Docsify Sidebar generated at $outFile" -ForegroundColor Green

