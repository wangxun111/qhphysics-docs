# 核心玩法层深度拆解 (Deep Dive: Core Gameplay Domain)

基于 `FishingFramework` 的代码结构与物理实现机制，我们将“核心玩法层”进一步细分为以下四个子系统。在针对手感、物理表现或钓鱼流程进行修改时，请精准定位到对应的子系统。

## 1. 物理仿真内核 (Physics Simulation Kernel)
*这是整个钓鱼体验的“物理引擎”，负责底层的纯数学与其力学计算。并不直接处理 Unity 的 GameObject，而是处理纯数据。*

*   **位置**: `FishingFramework/Physics`
*   **深度解析文档**: **[Deep Dive: Physics Kernel](./DeepDive_PhysicsKernel.md)**
*   **核心类**:
    *   `VerletFishBody.cs`, `VerletFishBendStrain.cs`: 使用 Verlet 算法模拟鱼的柔性身体摆动与受力形变。
    *   `SimulationThread.cs`: 物理计算似乎在独立线程或高频循环中运行，以保证计算精度。
*   **关键职责**: 模拟鱼体的真实摆动物理，而非简单的动画播放。

*   **柔性体与弹簧系统 (Soft Body & Spring System)**
    *   **核心类**:
        *   `FishingRodSimulation.cs`: 鱼竿弯曲的数学模型核心。
        *   `MassToRigidBodySpring.cs`: 模拟鱼线/鱼竿连接处的弹性阻尼关系（Hooke's Law + Damping）。
    *   **关键职责**: 计算竿梢的弯曲程度、鱼线的张力传递。这是解决“抖动”问题的源头。

## 2. 钓鱼逻辑实体 (Fishing Entities)
*将物理内核的计算结果映射到 Unity 世界的游戏对象 (GameObject) 上，处理具体的游戏行为。*

*   **竿组系统 (Rod Assembly)**
    *   **位置**: `FishingFramework/Module/Rod`
    *   **核心类**:
        *   `RodBehaviour.cs`: 鱼竿的主控逻辑。
        *   `BendingSegment.cs`: 鱼竿分节弯曲的表现控制。
        *   `RodPodController.cs`: 竿架系统（支架钓法支持）。
    *   **关键职责**: 根据 `FishingRodSimulation` 的数据更新鱼竿模型的骨骼/Mesh 形变。

*   **线轮系统 (Line & Reel System)**
    *   **位置**: `FishingFramework/Module/{Line, Reel}`
    *   **核心类**:
        *   `LineController.cs`: 鱼线渲染与物理节点同步。
        *   `ReelController.cs`: 卷线器逻辑（收线速度、卸力刹车 Friction）。
    *   **关键职责**: 处理玩家收线操作 (`ReelIn`)，计算当前的线长与张力反馈到物理内核。

*   **鱼类行为系统 (Fish Entity System)**
    *   **位置**: `FishingFramework/Module/Fish`
    *   **核心类**:
        *   `FishController.cs`: 鱼的实体控制。
        *   `FishAi.cs`: 鱼的 AI 决策（游动方向、挣扎力度）。
        *   `FishBite...*.cs`: 复杂的咬钩判定逻辑（吸入、吞食、吐钩）。
    *   **关键职责**: 决定鱼在水下的具体位置和状态，驱动物理模型移动。

## 3. 流程状态机 (State Management / FSM)
*管理钓鱼过程的生命周期，决定当前能不能操作，以及处于什么阶段。*

*   **位置**: `FishingFramework/Module/FSM`
*   **核心模块**:
    *   **PlayerState**: `Idle` -> `Casting` (抛竿) -> `Waiting` (等鱼) -> `Fighting` (搏鱼) -> `Landing` (提鱼)。
    *   **FishState**: `Roaming` (巡游) -> `Approaching` (靠近) -> `Biting` (咬钩) -> `Struggling` (挣扎)。
    *   **HookState**: 钩子的状态（在水下、在鱼嘴里、挂底）。
*   **关键职责**: 作为一个总控中心，协调 Input 和 Entity。例如：只有在 `Fighting` 状态下，`ReelController` 的刹车逻辑才生效。

## 4. 环境交互 (Environmental Interaction)
*处理实体与水体、地面的物理交互。*

*   **位置**: `FishingFramework/Module/{Water, Hit, Float}`
*   **核心类**:
    *   `WaterFXManager.cs`: 水花、波纹特效生成。
    *   `RigidBodyController.cs`: 刚体碰撞处理。
*   **关键职责**: 检测鱼饵入水、鱼跳出水面、鱼线切割水面的物理反馈。

---

### 开发指导 (Development Guidelines)

当您想解决类似“拉力抖动”的问题时：

1.  **第一步 (FSM Check)**: 检查 `PlayerState` 是否在 `Fighting` 状态正确切换，没有在 `Waiting` 和 `Fighting` 之间反复横跳。
2.  **第二步 (Entity Check)**: 检查 `ReelController` 的收线速度与 `RodBehaviour` 的弯曲更新是否同步。
3.  **第三步 (Physics Core)**: 如果表现层没问题，直接深入 `FishingFramework/Physics/MassToRigidBodySpring.cs`，调整阻尼系数 (Damping) 或弹簧系数 (Stiffness)。这是最根本的解决之道。
4.  **第四步 (Environment)**: 检查是否如之前所说，是因为 `Water` 表面的碰撞检测 (`Hit`) 导致了物理计算的突变。
