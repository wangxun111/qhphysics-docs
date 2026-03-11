# Docsify 侧边栏生成器
# 文件: G:\Copilot_OutPut\FishingGame\Scripts\GenerateSidebar.ps1

$rootDir = "G:\Copilot_OutPut\FishingGame"
$outDir = $rootDir # Sidebar 应在根目录
$outFile = "$outDir\_sidebar.md"

Write-Host ">>> Generating Sidebar..." -ForegroundColor Cyan

# 初始化 Sidebar 内容
$sidebarContent = "*   [🏠 首页 (Home)](/README.md)`n"

# 1. 重要根文件
$rootFiles = @("START_HERE.md", "QUICK_REFERENCE.md", "TESTING_GUIDE.md")
foreach ($f in $rootFiles) {
    if (Test-Path "$rootDir\$f") {
        $sidebarContent += "*   [📄 $f](/$f)`n"
    }
}

# 2. 知识库
if (Test-Path "$rootDir\KnowledgeBase") {
    $sidebarContent += "`n*   **📚 文档库 (KnowledgeBase)**`n"
    $files = Get-ChildItem -Path "$rootDir\KnowledgeBase" -Filter "*.md" | Sort-Object Name
    foreach ($file in $files) {
        $name = $file.Name.Replace(".md", "").Replace("_", " ")
        $path = "KnowledgeBase/$($file.Name)"
        $sidebarContent += "    *   [📄 $name](/$path)`n"
    }
}

# 3. HTML 文件夹 (包含可视化报表)
if (Test-Path "$rootDir\html") {
    $sidebarContent += "`n*   **📊 可视化报表 (HTML)**`n"
    $files = Get-ChildItem -Path "$rootDir\html" -Filter "*.html" | Sort-Object Name
    foreach ($file in $files) {
        if ($file.Name -eq "index.html") { continue }
        $name = $file.Name
        # 使用 ':ignore' 告诉 Docsify 不要拦截链接，而是作为外部资源打开/跳转
        $path = "html/$($file.Name)"
        $sidebarContent += "    *   [📊 $name](/$path ':ignore')`n"
    }
}

# 4. 脚本
if (Test-Path "$rootDir\Scripts") {
    $sidebarContent += "`n*   **🛠️ 工具脚本 (Scripts)**`n"
    $files = Get-ChildItem -Path "$rootDir\Scripts" -Filter "*.ps1" | Sort-Object Name
    foreach ($file in $files) {
        $name = $file.Name
        $path = "Scripts/$($file.Name)"
        $sidebarContent += "    *   [💻 $name](/$path ':ignore')`n"
    }
}

Set-Content -Path $outFile -Value $sidebarContent -Encoding UTF8
Write-Host ">>> Sidebar generated at $outFile with content length: $($sidebarContent.Length)" -ForegroundColor Green
# Write-Host ">>> Preview content:"
# Write-Output $sidebarContent

