# 代码可视化与分析工具思想参考 (Reference of Code Visualization & Analysis Tool Concepts)

既然您对简单的调用图（Call Graph）不满意，这里整理了一些业界成熟的代码分析与可视化工具的设计思想，这些思想可以被我们借鉴到当前的 AI 辅助工作流中。

## 1. 静态分析与依赖可视化 (Static Analysis & Dependency Visualization)

这些工具的核心思想是**解析代码结构（AST）**，然后以图形方式展示类、方法之间的静态关系。

### **NDepend** ( .NET 领域标杆)
*   **核心思想**:Code as Data (代码即数据). 将代码库视为数据库，使用 CQL (Code Query Language) 进行查询。
*   **可视化形式**:
    *   **Dependency Matrix (依赖矩阵)**: 展示复杂的依赖关系，特别是循环依赖。
    *   **TreeMap (矩形树图)**:通过矩形大小展示代码复杂度（Cyclomatic Complexity）或代码行数，通过颜色展示测试覆盖率。
    *   **Dependency Graph**: 类似于我们之前的尝试，但它会自动根据耦合度聚类。
*   **借鉴点**: 我们可以在生成文档时，不仅列出调用关系，还可以计算方法的"圈复杂度"（if/else 嵌套深度），并在图表中用颜色标记高风险方法。

### **Sourcetrail** (跨平台源码探索)
*   **核心思想**: 交互式探索。它不尝试一次性展示所有内容，而是从一个节点出发，动态加载其引用、继承和调用关系。
*   **可视化形式**: 三栏布局（代码编辑器、图形视图、搜索栏）。图形视图只显示当前关注符号的直接关系。
*   **借鉴点**: 生成 HTML 时，可以引入交互功能（如点击节点展开下一级，而不是一次性生成巨大的不可读图表）。

### **CodeMap** (Visual Studio Enterprise)
*   **核心思想**: 调试（Debug）辅助。它允许开发者在调试过程中，将断点涉及的方法动态添加到图表中，看到的是**运行时（Runtime）**的执行路径，而不仅仅是静态结构。
*   **借鉴点**: 我们的日志分析脚本（如 `AnalyzeJitter.ps1`）其实就是在做类似的事情——重现 Runtime 数据。我们可以将日志数据直接映射到流程图上，显示"实际走了哪条路"。

## 2. 文档生成与流程图 (Documentation & Flowcharts)

### **Doxygen** + **Graphviz**
*   **核心思想**:自动化提取注释和签名。Doxygen 可以自动生成"Call Graph"（调用谁）和"Caller Graph"（被谁调用）。
*   **借鉴点**: 它的图表非常严谨，包括类的继承关系。我们可以借鉴它的**自动链接**功能，点击图中的函数名跳转 to 代码定义。

### **PlantUML / Mermaid** (Text-to-Diagram)
*   **核心思想**: 文本即图表。便于版本控制。
*   **Sequence Diagram (时序图)**: 比流程图更适合展示对象之间的交互（Objects Interaction）。
*   **State Diagram (状态图)**: 对于 FSM（有限状态机）非常有效。
*   **借鉴点**: 对于 `FishingRodSimulation` 这种物理模拟，流程图（Flowchart）比时序图更合适；但对于 `PlayerState`，状态图是最佳选择。

## 3. 抽象语法树 (AST) 与控制流图 (CFG)

简单的正则表达式无法准确理解代码逻辑（如嵌套的 `if`）。业界工具通常使用 **AST 解析器**。

### **Roslyn** (C# Compiler API)
*   **核心思想**: 编译器作为服务。不但能编译，还能把代码解析成语法树供工具使用。
*   **应用**: IDE 的重构功能、代码高亮、错误检查都基于此。
*   **借鉴点**: 如果我们需要生成精确的逻辑流程图（包含 `if (load > friction)` 这样的条件分支），需要编写一个简易的 AST 解析脚本（Python `javalang` 库类似思想，或者 C# 自身的 `Microsoft.CodeAnalysis`）。

## 4. 我们的改进方案 (Action Plan)

基于由"Call Graph"（仅函数调用）向**"Control Flow Graph" (CFG, 控制流图)** 进化的思想，建议对工具链进行以下升级：

1.  **解析深度升级**: 不再只正则匹配 `MethodName()`, 而是识别 `if`, `else`, `return`, `switch` 关键字。
2.  **数据流标注**: 在箭头线上标注传递的参数或条件（例如：`ReelForce > Drag` --> `BreakLine`）。
3.  **交互式 HTML**: 用于展示的 HTML 应支持缩放和节点高亮（已部分实现，可加强）。

---
**参考链接 (模拟)**:
- NDepend: https://www.ndepend.com/
- Graphviz: https://graphviz.org/
- Mermaid Live Editor: https://mermaid.live/

