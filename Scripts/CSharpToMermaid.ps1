param(
    [string]$InputPath,
    [string]$OutputName = "CodeStructure"
)
if (-not $InputPath) {
    Write-Error "Usage: .\CSharpToMermaid.ps1 -InputPath 'F:\Path\To\Script.cs'"
    return
}
$FullPath = Resolve-Path $InputPath
$HtmlDir = "G:\Copilot_OutPut\FishingGame\html"
if (-not (Test-Path $HtmlDir)) { New-Item -ItemType Directory -Path $HtmlDir -Force | Out-Null }
$OutputPath = Join-Path $HtmlDir "$OutputName.html"
# Mermaid Header
$mermaidDef = "classDiagram`n"
# Helper to parse file
function Process-File($file) {
    $content = Get-Content $file -Raw
    # 1. Match Class Definition: public class Name : Base
    # Simple regex, handles basic inheritance
    $classPattern = '(?m)^\s*(?:public|private|protected|internal)?\s*(?:abstract|sealed|static|partial)*\s*class\s+(\w+)(?:\s*:\s*([^{\r\n]+))?'
    $matches = [regex]::Matches($content, $classPattern)
    foreach ($m in $matches) {
        $className = $m.Groups[1].Value
        $baseClasses = $m.Groups[2].Value
        # Add Class Node
        $script:mermaidDef += "    class $className`n"
        # Add Inheritance links
        if ($baseClasses) {
            $parents = $baseClasses -split ','
            foreach ($p in $parents) {
                $pName = $p.Trim()
                # Ignore interfaces starting with I usually (optional)
                if ($pName) {
                    $script:mermaidDef += "    $pName <|-- $className`n"
                }
            }
        }
        # 2. Match Public Methods (simplified)
        # public void MethodName(...)
        $methodPattern = '(?m)^\s*(public|protected)\s+(?:override|virtual|static|abstract)*\s*(?:\w+(?:<[^>]+>)?)\s+(\w+)\s*\('
        $methodMatches = [regex]::Matches($content, $methodPattern)
        foreach ($mm in $methodMatches) {
            $vis = $mm.Groups[1].Value
            $mName = $mm.Groups[2].Value
            $symbol = "+"
            if ($vis -eq "protected") { $symbol = "#" }
            $script:mermaidDef += "    $className : $symbol$mName()`n"
        }
        # 3. Match Public Properties/Fields
        $fieldPattern = '(?m)^\s*(public|protected)\s+(?:const|static|readonly)*\s*(\w+(?:<[^>]+>)?)\s+(\w+)\s*(?:;|=\{)'
        $fieldMatches = [regex]::Matches($content, $fieldPattern)
        foreach ($fm in $fieldMatches) {
             # Basic property matching
             $vis = $fm.Groups[1].Value
             $type = $fm.Groups[2].Value
             $fName = $fm.Groups[3].Value
             $symbol = "+"
             if ($vis -eq "protected") { $symbol = "#" }
             $script:mermaidDef += "    $className : $symbol$type $fName`n"
        }
    }
}
if (Test-Path $FullPath -PathType Container) {
    Get-ChildItem $FullPath -Filter "*.cs" | ForEach-Object { Process-File $_.FullName }
} else {
    Process-File $FullPath
}
# HTML Template
$htmlTemplate = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Code Structure: $OutputName</title>
    <style>body { font-family: sans-serif; padding: 20px; }</style>
</head>
<body>
    <h2>Code Structure Visualization</h2>
    <p>Source: $InputPath</p>
    <pre class="mermaid">
$mermaidDef
    </pre>
    <script type="module">
        import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';
        mermaid.initialize({ startOnLoad: true });
    </script>
</body>
</html>
"@
$htmlTemplate | Out-File -FilePath $OutputPath -Encoding UTF8
Write-Host "Diagram generated at: $OutputPath"
