# AI 辅助开发最佳实践流程 (AI-Assisted Development Best Practices)
**位置**: `G:\Copilot_OutPut\FishingGame\KnowledgeBase`

本文档总结了当前业界常用的 AI 辅助编程工作流，旨在为未来的开发提供扩展思路。

## 1. 需求分析与架构设计 (Design Phase)
- **伪代码原型 (Pseudocode Prototyping)**
  - *流程*: 用自然语言描述复杂逻辑（如物理计算流程），让 AI 转换为结构化的伪代码或接口定义。
  - *适用*: 新功能开发初期，如设计新的鱼类行为树。
- **技术方案评审 (Architecture Review)**
  - *流程*: 将拟定的类图或数据结构发给 AI，询问潜在的扩展性问题或性能瓶颈。

## 2. 编码实施 (Implementation Phase)
- **注释驱动开发 (Comment-Driven Development)**
  - *流程*: 开发者写下详细的函数头注释（输入、输出、副作用），AI 自动补全函数体。
  - *适用*:通过 Copilot 插件在 IDE 中实时完成。
- **样板代码生成 (Boilerplate Generation)**
  - *流程*: 让 AI 生成重复性高的代码，如 UI 事件绑定、数据解析类、配置读取器。

## 3. 测试与���量保证 (Testing & QA)
- **单元测试生成 (Unit Test Generation)**
  - *流程*: 选中核心业务逻辑代码（如 `Reel1stBehaviour.cs` 中的计算公式），让 AI 生成对应的 NUnit 测试用例，覆盖边界条件。
  - *价值*: 确保物理公式在重构时的稳定性。
- **自动化测试数据构造**
  - *流程*: 让 AI 生成各种极端情况下的配置数据（如极大的鱼重、极小的摩擦力），用于压力测试。

## 4. 调试与维护 (Debugging & Maintenance)
- **日志智能分析 (Log Intelligence)** *(本项目已实施)*
  - *流程*: 编写脚本（如 `AnalyzeJitter.ps1`）预处理日志，让 AI 分析数据趋势和异常点。
- **代码重构 (Refactoring)**
  - *流程*: 让 AI 识别"魔法数字"、过长函数、紧耦合代码，并提出重构方案（如本项目中提取常量的操作）。
- **错误排查 (Error Diagnosis)**
  - *流程*: 直接将报错堆栈 (Stack Trace) 和相关代码上下文发给 AI，获取修复建议。

## 5. 文档与知识管理 (Documentation)
- **代码自文档化 (Auto-Documentation)**
  - *流程*: 让 AI 读取复杂模块代码，生成 Markdown 格式的技术文档或 API 手册。
- **上下文快照 (Context Snapshotting)** *(本项目已实施)*
  - *流程*: 维护活跃上下文文件，确保 AI 会话间的记忆连续性。

## 🚀 建议引入本项目的下一步流程
1. **为核心物理计算引入单元测试**: 既然 `Reel1stBehaviour` 已经稳定，可以考虑让 AI 生成测试用例来“固化”这一逻辑，防止未来由于意外修改导致回归。
2. **自动化文档更新**: 当代码修改后，让 AI 自动更新 `KnowledgeBase` 中的对应文档。

