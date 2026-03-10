# CSharpToFlowchart.ps1
# Advanced Flowchart Generator based on "Control Flow Graph" concepts
# Usage: .\CSharpToFlowchart.ps1 -SourceFile "path/to/script.cs" -MethodName "Simulate"

param(
    [string]$SourceFile = "F:\new\fishinggame\Assets\FishingFramework\Physics\FishingRodSimulation.cs",
    [string]$MethodName = "Simulate",
    [string]$OutputDir = "G:\Copilot_OutPut\FishingGame\html"
)

if (-not (Test-Path $SourceFile)) {
    Write-Error "Source file not found: $SourceFile"
    exit
}

$lines = Get-Content $SourceFile

# Simple regex extractor for method body (Assuming standard Formatting)
# This is a heuristic parser, not a full compiler
$startLine = -1
$endLine = -1
$braceCount = 0
$foundMethod = $false

for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i].Trim()
    
    # Remove comments for simple parsing
    $cleanLine = $line -replace "//.*$", ""
    
    if (-not $foundMethod) {
        # Strict check using wildcards to avoid regex issues
        if ($line -like "*void $MethodName(*" -or $line -like "*void $MethodName (*") {
            Write-Host "Found method '$MethodName' start at line $($i+1)"
            $startLine = $i
            $foundMethod = $true
            # Check for opening brace on the same line
            if ($cleanLine.Contains("{")) { 
                $braceCount += ($cleanLine.ToCharArray() | Where-Object { $_ -eq '{' }).Count
            }
        }
    } else {
        # Rudimentary brace counting on clean line (ignoring strings for now is risky but better than nothing)
        # Use simple replace to kill braces inside strings? Too complex for regex without loop.
        # Just strip comments first.
        
        $openBraces = ($cleanLine.ToCharArray() | Where-Object { $_ -eq '{' }).Count
        $closeBraces = ($cleanLine.ToCharArray() | Where-Object { $_ -eq '}' }).Count
        $braceCount += ($openBraces - $closeBraces)
        
        # Write-Host "Line $($i+1): Braces $braceCount (Open: $openBraces, Close: $closeBraces)"
        
        if ($braceCount -eq 0 -and ($openBraces -gt 0 -or $closeBraces -gt 0)) {
            $endLine = $i
            break
        }
    }
}

if ($startLine -eq -1 -or $endLine -eq -1) {
    Write-Error "Method '$MethodName' not found or could not parse bounds."
    exit
}

# Analyze the body lines (Simulated Parser)
$bodyLines = $lines[($startLine+1)..($endLine-1)]
$mermaidLines = @()
$mermaidLines += "flowchart TD"
$mermaidLines += "    Start($MethodName) --> Node0"

$nodeCounter = 0
$lastNode = "Node0"

foreach ($rawLine in $bodyLines) {
    $line = $rawLine.Trim()
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    if ($line.StartsWith("//")) { continue }

    $currentNode = "Node$($nodeCounter+1)"
    $label = ""
    $shapeStart = "["
    $shapeEnd = "]"

    # Detect Control Flow
    if ($line -match "if\s*\((.*)\)") {
        $condition = $matches[1].Replace('"', "'").Replace('<', '&lt;').Replace('>', '&gt;')
        $label = "If: $condition"
        $shapeStart = "{{"
        $shapeEnd = "}}"
    }
    elseif ($line -match "else") {
        # Skip else for linear graph
        continue 
    }
    elseif ($line -match "return") {
        $label = "Return"
        $shapeStart = "(("
        $shapeEnd = "))"
    }
    # Detect Method Calls (PascalCase followed by paren)
    elseif ($line -match "\b([A-Z]\w+)\(") {
        $calledMethod = $matches[1]
        # Ignore common Unity/System calls for clarity unless important
        if ($calledMethod -notmatch "^(Debug|Log|Mathf|Vector3|Transform|GameObject)$") {
            $label = "Call: $calledMethod"
        }
    }

    if ($label -ne "") {
        $safeLabel = $label
        $mermaidLines += "    $lastNode --> $currentNode"
        $mermaidLines += "    $currentNode$shapeStart`"$safeLabel`"$shapeEnd"
        $lastNode = $currentNode
        $nodeCounter++
    }
}
$mermaidLines += "    $lastNode --> End((End))"

# Generate Mermaid
$mermaidContent = $mermaidLines -join "`n"

# Create HTML
$htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Flow Analysis: $MethodName</title>
    <style>body { font-family: sans-serif; padding: 20px; }</style>
</head>
<body>
    <h2>Advanced Control Flow: $MethodName</h2>
    <p><strong>Source:</strong> $SourceFile</p>
    <p>Generated based on heuristic AST analysis.</p>
    <div class="mermaid">
$mermaidContent
    </div>
    <script type="module">
        import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';
        mermaid.initialize({ startOnLoad: true });
    </script>
</body>
</html>
"@

$outputFile = Join-Path $OutputDir "${MethodName}_Flow_Advanced.html"
Set-Content -Path $outputFile -Value $htmlContent
Write-Host "Generated flow chart: $outputFile"
