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
