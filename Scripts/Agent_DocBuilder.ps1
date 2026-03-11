# Agent 1: Doc Builder
# Role: Converts Markdown to HTML with Mermaid Diagram Support
# File: G:\Copilot_OutPut\FishingGame\Scripts\Agent_DocBuilder.ps1
param(
    [string]$TargetFile = ""
)
$rootDir = "G:\Copilot_OutPut\FishingGame"
# Define the HTML template using Single-Quoted Here-String to allow $ in JS
$htmlTemplate = @'
<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="UTF-8">
    <title>{{TITLE}}</title>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/github-markdown-css/5.1.0/github-markdown-light.min.css">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.5.0/styles/github.min.css">
    <style>
        body { box-sizing: border-box; min-width: 200px; max-width: 980px; margin: 0 auto; padding: 45px; }
        .mermaid { margin: 20px 0; text-align: center; }
    </style>
</head>
<body>
    <div id="content" class="markdown-body"></div>
    <!-- Dependencies -->
    <script src="https://cdn.jsdelivr.net/npm/marked/marked.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.5.0/highlight.min.js"></script>
    <script type="module">
        import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';
        mermaid.initialize({ startOnLoad: false });
        const rawMd = {{JSON_CONTENT}};
        // 1. Render Markdown to HTML
        document.getElementById('content').innerHTML = marked.parse(rawMd);
        hljs.highlightAll();
        // 2. Post-Process for Mermaid
        // marked.js renders ```mermaid as <pre><code class="language-mermaid">
        const mermaidBlocks = document.querySelectorAll('pre code.language-mermaid');
        mermaidBlocks.forEach((block, index) => {
            const pre = block.parentElement;
            const code = block.textContent;
            // Create separate div for Mermaid
            const div = document.createElement('div');
            div.className = 'mermaid';
            div.textContent = code;
            // Replace <pre> with <div>
            pre.replaceWith(div);
        });
        // 3. Trigger Mermaid Render
        mermaid.run();
    </script>
</body>
</html>
