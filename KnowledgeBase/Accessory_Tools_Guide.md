# 工具链文档 (Toolchain Documentation)
**位置**: `G:\Copilot_OutPut\FishingGame\Scripts`

此目录包含用于辅助开发、调试和日志分析的自动化脚本。

## 📋 工具索引 (Tool Index)

| 序号 | 工具名称 | 脚本/类型 | 输出形式 | 适用场景与区别 |
| :--- | :--- | :--- | :--- | :--- |
| 1 | **物理震荡分析器** | `AnalyzeJitter.ps1` | `Console/Text` | **日志分析**：专用于分析 1档卷线器物理日志 jitter 数据。 |
| 2 | **文档可视化查看器** | `ConvertMdToHtml.ps1` | `HTML` | **文档阅读**：将 Markdown 变为好看的网页。 |
| 3 | **类结构透视 (Structure)** | `CSharpToMermaid.ps1` | `Class Diagram` | **静态蓝图**：查看类的继承关系、接口实现和成员变量。*(关注“是什么”)* |
| 4 | **业务逻辑复盘 (Logic)** | **AI Assisted** | `Interactive Flowchart` | **深度导航**：AI 分析生成的复杂业务逻辑、算法流和状态跳转。*(关注“怎么运行”)* |
| 5 | **文档双向同步** | `SyncDocs.ps1` | `Git Ops` | **云端备份**：一键执行 `Commit -> Pull Rebase -> Push` 同步到 GitHub Pages。 |

## 🛠️ 当前可用工具

### 1. 物理震荡分析器 (AnalyzeJitter.ps1)
- **文件路径**: `G:\Copilot_OutPut\FishingGame\Scripts\AnalyzeJitter.ps1`
- **功能**: 
  - 自动扫描指定的物理日志文件（如 `fishing_phy_log_*.txt`）。
  -  检测“线长变化方向翻转”（Direction Flips）的次数。
  - 计算翻转比例（Jitter Severity），用于量化评估 1档卷线器的震荡程度。
- **使用方法**:
  1. 打开 PowerShell 终端。
  2. 运行命令：
     ```powershell
     & "G:\Copilot_OutPut\FishingGame\Scripts\AnalyzeJitter.ps1"
     ```
  3. 注意：脚本内部默认指向 `F:\new\fishinggame\fishing_phy_log_20260309_200524.txt`。如果需要分析新日志，请用文本编辑器打开脚本修改 `$path` 变量。

### 2. 文档可视化查看器 (DocViewer) (Automated)
- **文件路径**: `G:\Copilot_OutPut\FishingGame\Scripts\ConvertMdToHtml.ps1`
- **功能**: 
  - 自动将指定的 `.md` 文件转换为风格精美的 `.html` 网页。
  - 自动应用项目统一的 CSS 样式。
  - 输出路径自动指向 `html` 目录。
- **使用方法**:
  1. 打开 PowerShell。
  2. 运行命令：
     ```powershell
     & "G:\Copilot_OutPut\FishingGame\Scripts\ConvertMdToHtml.ps1" -InputPath "G:\Copilot_OutPut\FishingGame\KnowledgeBase\YourDoc.md"
     ```
  3. 转换后的网页将生成在 `G:\Copilot_OutPut\FishingGame\html\YourDoc.html`。

  - 流程设计: 未来所有重要 Markdown 文档都应配有一个对应的 HTML 版本存放在 `html` 目录，通过脚本自动转换（待实现）。

### 3. 类结构透视 (StructureViewer)
- **文件路径**: `G:\Copilot_OutPut\FishingGame\Scripts\CSharpToMermaid.ps1`
- **功能**: 生成类的**静态结构图** (Class Diagram)。
- **用途**: 关注“**这个类包含什么**”——成员变量类型、继承父类、接口实现等。
- **使用方法**: PowerShell 命令分析单个文件。

### 4. 业务逻辑复盘 (LogicFlow Visualizer - AI Assisted)
- **生成方式**: AI 深度分析 (手动触发)
- **功能**: 生成全屏交互式的**复杂业务逻辑流程图**。
- **用途**: 关注“**核心逻辑怎么运行**”——AI 会理解代码意图，画出完整的物理计算公式、状态机跳转、LOD 切换条件等。
- **优势**: 包含详细的决策分支和数据流向，支持全屏拖拽缩放，适合复杂算法分析。
- **产物**: `G:\Copilot_OutPut\FishingGame\html\*.html`

### 5. 文档双向同步 (Sync Docs to GitHub)
- **文件路径**: `G:\Copilot_OutPut\FishingGame\SyncDocs.bat` (快捷方式)
- **脚本路径**: `G:\Copilot_OutPut\FishingGame\Scripts\SyncDocs.ps1`
- **功能**:
  - 自动暂存 (`git add .`) 并提交本地更改。
  - 从远程仓库拉取更新 (`git pull --rebase`)。
  - 将最新文档推送到 GitHub (`git push`)。
- **配置**:
  - 远程仓库: `https://github.com/wangxun111/qhphysics-docs`
  - 部署地址: `https://wangxun111.github.io/qhphysics-docs/`
- **使用方法**:
  - 双击运行 `G:\Copilot_OutPut\FishingGame\SyncDocs.bat` 即可。
  - 首次运行可能需要输入 GitHub 账号密码或 Token。

## 📚 外部参考资料 (References)
- **代码可视化工具思想库**: [Code_Visualization_Ideas.md](G:\Copilot_OutPut\FishingGame\KnowledgeBase\Code_Visualization_Ideas.md)
  - *内容*: 包含了 NDepend, CodeMap, Doxygen 等业界标准工具的设计思想，用于指导后续工具开发。

---
*未来添加的新工具请记录于此。*
