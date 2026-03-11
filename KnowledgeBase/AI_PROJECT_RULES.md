# AI Agent Project Rules

ATTENTION AI: Please read this file immediately and strictly follow the project rules defined herein.

## 1. Output Directory for Artifacts
**CRITICAL RULE**: Do not save temporary analysis files, logs, summaries, or non-code artifacts into the project directory.

**TARGET PATHS**:
- **Knowledge Base (Summaries/Docs):** `G:\Copilot_OutPut\FishingGame\KnowledgeBase`
- **Web/HTML Reports:** `G:\Copilot_OutPut\FishingGame\html`
- **Logs & Analysis:** `G:\Copilot_OutPut\FishingGame\Log`

**DYNAMIC FOLDER CREATION**:
- If a generated file does not belong to the categories above, determine a new specific category folder name (e.g., `Scripts`, `Data`) or ask the user for clarification.
- Create the new directory at `G:\Copilot_OutPut\FishingGame\<NewCategory>`.
- Save the file in the new directory.

- All generated summary reports must be saved to the Knowledge Base.
- All log analysis files must be saved to the Log folder.
- Do not clutter the `F:\new\fishinggame` source tree.

## 2. Physics Tuning Context (Reel1stBehaviour)
- **Gear 1 (Low Friction)**:
  - Strict Deadzone logic is applied: If Load > 80% Friction, Reeling Speed = 0.
  - Dynamic Gain for Drag Out: Low gain for small forces (anti-jitter), high gain for large forces (anti-disconnect).

## 3. Language Requirements
**CRITICAL RULE**: All responses and explanations must be provided in **Chinese** (Simplified).

## 4. Project Knowledge Base & History
The repository of technical decisions, problem analysis, and thought processes is stored externally to keep the source clean.
**LOCATION**: `G:\Copilot_OutPut\FishingGame\KnowledgeBase`

**ACTIVE CONTEXT SNAPSHOT**:
- Before starting work, check `G:\Copilot_OutPut\FishingGame\KnowledgeBase\Active_Context_Snapshot.md`.
- This file contains the live status of the previous session and pending tasks.
- If you complete significant work, update this file to reflect the new state.

Before modifying complex systems, check this folder for existing context.

### Key Index:
- **Session History (2026-03-10)**: See `G:\Copilot_OutPut\FishingGame\KnowledgeBase\Session_History_Summary_20260310.md`
  - *Content*: Comprehensive summary of the "Gear 1 Jitter" fix, failed attempts, final solution reasoning, and infrastructure setup.
  - *Action*: Read this first to understand the current state of reel physics.

- **Reel Physics Jitter Fix (Gear 1)**: See `G:\Copilot_OutPut\FishingGame\KnowledgeBase\Reel_Physics_Fix_Summary.txt`
  - *Context*: Solved the high-frequency oscillation of rod tip and line length in low friction settings.
  - *Solution*: Implemented strict Deadzone for reeling and Dynamic Gain for drag out.
  - *Status*: Stable. Do not revert without reading this summary.

- **Accessory Tools Guide**: See `G:\Copilot_OutPut\FishingGame\KnowledgeBase\Accessory_Tools_Guide.md`
  - *Content*: Documentation for utility scripts like `AnalyzeJitter.ps1`.
  - *Action*: Consult this guide when needing to perform log analysis or debugging tasks.

- **AI Workflow Best Practices**: See `G:\Copilot_OutPut\FishingGame\KnowledgeBase\AI_Workflow_Best_Practices.md`
  - *Content*: Recommended workflows for AI-assisted development, including testing strategies and documentation.
  - *Action*: Review for guidance on efficient collaboration with AI.

## 5. Debugging Standards
**CRITICAL RULE**: The path/location for debug options is considered to be in **"medium 的 AI"** (Medium's AI).
- When looking for or documenting debug options, refer to this location.

## 🤖 多 Agent 协同与自动化工作流 (New)

### 1. 核心架构：意图驱动 (Intent-Driven)

项目采用 **"Orchestrator-Worker"** 的多 Agent 协作模式，以实现高度自动化的开发流程。

*   **🧠 意图中心 (`ProjectIntent.json`)**:
    *   定义当前的工作模式（如 `Development`, `Writing`, `Debugging`）。
    *   控制各个子 Agent 的开关和参数。
    *   **规则**: 在开始大规模任务前，检查并调整此文件以匹配当前目标。

*   **指挥官 (`TaskOrchestrator.ps1`)**:
    *   负责监听文件变更和系统状态。
    *   **规则**: 不直接处理业务逻辑，只负责分发任务给子 Agent。
    *   **启动**: 使用 `Start-Process` 在独立窗口运行，保持长驻。

### 2. 角色分工 (Agents)

| Agent | 职责 | 触发条件 | 输出 |
| :--- | :--- | :--- | :--- |
| **DocBuilder** | Markdown -> HTML 转换 | `.md` 文件变更 | 生成同名 `.html` 文件 |
| **Navigator** | 扫描目录 -> 更新导航索引 | HTML 生成完毕 / 新报表出现 | 更新 `site_nav.js` |
| **LogDetective**| 物理日志分析 | 发现 `fishing_phy_log_*.txt` | 调用分析脚本生成报表 |

### 3. 使用规范

1.  **文档编写**:
    *   直接在 `KnowledgeBase/` 下创建或修改 Markdown。
    *   保存后，**DocBuilder** 会自动构建，**Navigator** 会自动更新索引。
    *   无需手动运行构建脚本。

2.  **文件修改安全协议 (Guardrail Protocol)**:
    *   **CRITICAL RULE**: 严禁使用 `New-Item -Force` 覆盖任何已存在的非临时文件。
    *   **CRITICAL RULE**: 修改关键文档或代码前，必须先使用 `Test-Path` 检查。
    *   **CRITICAL RULE**: 必须在写入后立即验证文件大小 (`.Length > 0`)，防止写入空内容。
    *   **推荐**: 对于高风险覆盖操作，使用 `Tools/SafeWrite.ps1` (如果存在) 或先备份 (`ComplextFile.md.bak`)。

3.  **日志分析**:
    *   将游戏日志放入项目根目录。
    *   **LogDetective** 会自动发现并生成分析报告（HTML 格式）。
    *   在文档中心的 "📊 可视化报表" 栏目查看结果。

4.  **调试与干预**:
    *   若需暂停自动化，修改 `ProjectIntent.json` 中的 `enabled: false`。
    *   若需全量重建，删除 `site_nav.js` 或运行 `Scripts/BuildSite.ps1` (手动触发)。

5.  **文件命名与防覆盖**:
    *   **CRITICAL RULE**: 严禁在根目录创建 `Index.md` 或 `REAMIE.md`，这会导致 `index.html` 被覆盖而损坏门户。请使用 `Home.md` 作为文档首页。
    *   **Encoding**: 所有 PowerShell 脚本必须使用 UTF-8 BOM 保存，注释必须使用英文，禁止中文乱码导致语法崩溃。
    *   **JSON Integrity**: 生成 HTML 时，必须强制转换内容为 `[string]`，防止 PowerShell 对象属性泄露。
