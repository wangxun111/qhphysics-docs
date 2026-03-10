# 钓鱼项目知识库索引 (Project Knowledge Base Index)

本索引旨在帮助开发者与 AI 快速定位项目文档与技术沉淀，构建清晰的上下文关联。

## 1. 项目架构与概览 (Architecture & Overview)
*   **[全项目分层架构拆解](./Project_Structure_Decomposition.md)**
    *   **文件**: `Project_Structure_Decomposition.md`
    *   **内容**: 将项目分为核心玩法、表现、架构、外围系统、平台安全等五大领域。
    *   **用途**: 明确任务所属层级，用于 AI Prompt 上下文设定，提高回答精准度。

*   **[核心玩法层深度拆解](./Core_Gameplay_DeepDive.md)**
    *   **文件**: `Core_Gameplay_DeepDive.md`
    *   **内容**: 物理内核 (Verlet)、逻辑实体 (Rod/Reel)、状态机 (FSM) 的详细分析。
    *   **用途**: 解决手感优化、物理仿真、状态同步等核心问题的导航地图。

*   **[物理仿真内核深度解析](./DeepDive_PhysicsKernel.md)**
    *   **文件**: `DeepDive_PhysicsKernel.md`
    *   **内容**: Verlet 积分求解器、软体鱼骨架 (Tetrahedron)、刚体混合物理接口 (MassToRigidBodySpring) 的实现细节。
    *   **核心发现**: 揭示了路亚（Topwater Lure）针对水面有特殊的垂直脉冲系数 (`ImpulseVerticalFactor`)，直接关联“水面弹跳”问题。
    *   **用途**: 深入代码级的物理参数调优参考。

## 2. 技术深度分析与问题复盘 (Deep Dives & Retrospectives)
*   **[拉力抖动根因分析与解决方案](./KnowledgeBase/01_Fishing_Mechanics_DeepDive.md)**
    *   **文件**: `KnowledgeBase/01_Fishing_Mechanics_DeepDive.md`
    *   **内容**: 关于 `friction=1.1772` 抖动问题的完整排查记录。
    *   **关键结论**: 确认“封层弹跳”为物理根因，采用“积分判定方案”滤除高频噪声。

## 3. 工作流与协作规范 (Workflow & Standards)
*   **[AI 协作开发工作流](./AI_Collaboration_Workflow.md)**
    *   **文件**: `AI_Collaboration_Workflow.md`
    *   **内容**: 如何拆解任务、ODDA 循环调试法 (Observation-Data-Design-Analyze)、Prompt 最佳实践。
    *   **用途**: 提升与 AI 结对编程的效率，规范提问方式。

---
*Created by GitHub Copilot on 2026-03-05*
