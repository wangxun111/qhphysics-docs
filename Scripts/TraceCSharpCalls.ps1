param(
    [string]$InputPath,
    [string]$EntryMethod = "Simulate",
    [string]$OutputName = "MethodFlow"
)
if (-not $InputPath) {
    Write-Error "Usage: .\TraceCSharpCalls.ps1 -InputPath 'path\to\file.cs' -EntryMethod 'Update'"
    return
}
$FullPath = Resolve-Path $InputPath
$HtmlDir = "G:\Copilot_OutPut\FishingGame\html"
if (-not (Test-Path $HtmlDir)) { New-Item -ItemType Directory -Path $HtmlDir -Force | Out-Null }
$OutputPath = Join-Path $HtmlDir "$OutputName.html"
# 1. Read File
$code = Get-Content $FullPath -Raw
# 2. Extract all method names AND signatures
# Pattern: Group 1=Modifiers, Group 2=Type, Group 3=Name
$methodPattern = '(?m)^\s*((?:(?:public|private|protected|internal|override|virtual|static|async|extern|new|readonly|unsafe|volatile)\s+)*)([\w<>[\]?]+)\s+(\w+)\s*\('
$methodInfo = @{}

$matches = [regex]::Matches($code, $methodPattern)
foreach ($m in $matches) {
    $modifiers = $m.Groups[1].Value.Trim()
    $type = $m.Groups[2].Value
    $name = $m.Groups[3].Value
    
    if ($name -ni "if", "for", "foreach", "while", "switch", "catch") {
        $methodInfo[$name] = @{
            Modifiers = $modifiers
            Type = $type
        }
    }
}
$allMethods = $methodInfo.Keys

# 3. Helper to extract method body
function Get-MethodBody {
    param($methodName, $sourceCode)
    # Search for definition
    # Corrected regex to handle multiple modifiers with spaces (e.g. "protected override")
    $defRegex = [regex]"(?m)^\s*(?:(?:public|private|protected|internal|override|virtual|static|async|extern|new|readonly|unsafe|volatile)\s+)*[\w<>[\]?]+\s+$methodName\s*\("
    $match = $defRegex.Match($sourceCode)
    if (-not $match.Success) { return $null }
    $startIndex = $match.Index
    $openBraceIndex = $sourceCode.IndexOf("{", $startIndex)
    if ($openBraceIndex -eq -1) { return $null }
    # Simple brace counting
    $braceCount = 1
    $i = $openBraceIndex + 1
    $len = $sourceCode.Length
    while ($i -lt $len -and $braceCount -gt 0) {
        $c = $sourceCode[$i]
        if ($c -eq '{') { $braceCount++ }
        elseif ($c -eq '}') { $braceCount-- }
        $i++
    }
    if ($braceCount -eq 0) {
        return $sourceCode.Substring($openBraceIndex, $i - $openBraceIndex)
    }
    return $null
}
# 4. Build Call Graph (Depth 1 for now to safely avoid infinite recursion loops in regex)
$mermaidGraph = "flowchart TD`n"
# Define styles
$mermaidGraph += "    classDef publicNode fill:#d4edda,stroke:#28a745,stroke-width:2px,color:#155724;`n"
$mermaidGraph += "    classDef protectedNode fill:#fff3cd,stroke:#ffc107,stroke-width:2px,color:#856404;`n"
$mermaidGraph += "    classDef privateNode fill:#f8d7da,stroke:#dc3545,stroke-width:2px,color:#721c24;`n"
$mermaidGraph += "    classDef defaultNode fill:#e2e3e5,stroke:#383d41,stroke-width:2px,color:#383d41;`n"

$processed = @{}
$queue = New-Object System.Collections.Generic.Queue[string]
$queue.Enqueue($EntryMethod)

Write-Output "Queue Type: $($queue.GetType().FullName)"

# Limit depth/complexity
$maxNodes = 20
$nodeCount = 0

while ($queue.Count -gt 0 -and $nodeCount -lt $maxNodes) {
    $current = $queue.Dequeue()
    
    if ($processed.ContainsKey($current)) { continue }
    $processed[$current] = $true
    
    # Node Styling & Labeling
    $info = $methodInfo[$current]
    $label = $current
    $styleClass = "defaultNode"
    
    if ($info) {
        $vis = "private"
        if ($info.Modifiers -match "public") { $vis = "public"; $styleClass = "publicNode" }
        elseif ($info.Modifiers -match "protected") { $vis = "protected"; $styleClass = "protectedNode" }
        elseif ($info.Modifiers -match "private") { $vis = "private"; $styleClass = "privateNode" }
        elseif ($info.Modifiers -match "internal") { $vis = "internal"; $styleClass = "defaultNode" }
        
        # HTML label support in Mermaid string
        $displayMods = if ($info.Modifiers) { $info.Modifiers } else { "private" }
        $label = "`"$current<br/><sub>$displayMods $($info.Type)</sub>`""
    } else {
        $label = "$current" 
    }
    
    $mermaidGraph += "    $current($label):::$styleClass`n"
    
    Write-Output "Processing method: $current"
    $body = Get-MethodBody -methodName $current -sourceCode $code
    if ($body) {
        Write-Output "  > Body found. Size: $($body.Length)"
        foreach ($target in $allMethods) {
            if ($target -eq $current) { continue }
            
            # Check if target is called in body
            # Simple check: MethodName(
            if ($body -match "\b$target\s*\(") {
                Write-Output "  > Found call to: $target"
                $mermaidGraph += "    $current --> $target`n"
                if (-not $processed.ContainsKey($target)) {
                    $queue.Enqueue($target)
                }
            }
        }
    } else {
        Write-Output "  > Body NOT found for $current"
    }
    $nodeCount++
}
# 5. Output HTML
$htmlTemplate = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Flow Analysis: $OutputName</title>
    <style>body { font-family: sans-serif; padding: 20px; }</style>
</head>
<body>
    <h2>Execution Flow Visualization (Call Graph)</h2>
    <p><strong>Entry Point:</strong> $EntryMethod</p>
    <p><strong>Source:</strong> $InputPath</p>
    <div class="mermaid">
$mermaidGraph
    </div>
    <script type="module">
        import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';
        mermaid.initialize({ startOnLoad: true });
    </script>
</body>
</html>
"@
$htmlTemplate | Out-File -FilePath $OutputPath -Encoding UTF8
Write-Host "Flow chart generated at: $OutputPath"
