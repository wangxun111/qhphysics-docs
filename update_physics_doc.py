
import os

content = r"""# 物理仿真内核深度解析 (Deep Dive: Physics Kernel)

基于对核心代码的静态分析，本类文档详细拆解了 FishingGame 项目中自定义物理引擎的实现原理。该系统混合了 **Verlet 积分（软体/绳索）** 与 **刚体动力学（Rigidbody）**，通过自定义求解器实现高频交互。

## 1. 架构总览 (Architecture Overview)
物理内核并非运行在 Unity 的 `FixedUpdate` 中，而是通过 `SimulationThread` 在独立线程（或受控的循环）中运行，以实现比 Unity PhysX 更高的迭代频率和确定性。

*   **入口**: `SimulationThread.cs`
*   **世界上下文**: `FishingRodSimulation.cs` (管理全局参数与实体引用)
*   **求解方式**: 显式积分 (Explicit Integration) + 约束投影 (Constraint Projection)

## 2. 核心组件解析 (Component Analysis)

### A. 仿真回路 (The Simulation Loop) - `SimulationThread.cs`
这是物理世界的“心跳”。
*   **双缓冲机制**: 使用 `ParticleSnapshot[]` 存储计算结果，供 Unity 主线程渲染使用，通过 `Snapshot` 和 `SyncLock` 保证线程安全。
*   **指令队列 (Action Queue)**: 主线程不直接修改物理对象，而是发送 `ParticleAction`（如 `EParticleActionType.Force`, `StopLine`）到队列中，由仿真线程在下一帧统一处理。
*   **时间步进**: 使用精确的 `DeltaTime` 和 `MaxIterationsPerFrame` (默认 150) 来切分时间步，保证绳索和软体的稳定性。

### B. 动力驱动机制 (The Force Drive Mechanism) - `Logic -> Physics`
这是鱼如何“用力”的核心链路，展示了逻辑层如何驱动物理层。这个机制决定了鱼的挣扎力度，直接影响手感。

1.  **决策层 (`FishActionNode.cs`)**:
    *   **职责**: 行为树节点，决定鱼在当前时刻的“目标力道”。
    *   **平滑处理**: 使用 `Mathf.SmoothDamp` (`UpdateForceSmoothing`) 将力的变化平滑化，避免物理系统受到阶跃信号冲击而爆炸。
    *   **动态调整**: `ChangeForceDynamic` 模拟博鱼过程中的挣扎，根据 `RealBiteAction.Proportion` 周期性、动态地调整目标力。
    *   **核心逻辑**:
        ```csharp
        // 动态计算目标力
        float buff = actionData.RealBiteAction.ProportionMax ...;
        SetForceTarget(actionData.InitForce.Value * (buff * 0.01f) * 9.81f);
        
        // 平滑逼近
        actionData.AI.Force = Mathf.SmoothDamp(..., smoothTime, ...);
        ```

2.  **数据层 (`FishAi.cs`)**:
    *   **职责**: 数据的中转站，持有 `Force` (即 `CurrentForce`) 属性。AI 逻辑修改此属性，物理控制器读取此属性。

3.  **执行层 (`VerletFishThrustController.cs`)**:
    *   **职责**: 物理引擎内部的 `Connection`。
    *   **力学实现**:
        它将 `Force` 转换为 `WaterMotor` 施加给鱼身体的质点。
        ```csharp
        Vector4f vector4f2 = Force * new Vector4f(body.UnderwaterRatio); // 根据在水下的比例打折
        mouth.WaterMotor4f = vector4f2 + ...; // 施加到嘴巴质点
        root.WaterMotor4f = ...;       // 反向施加到尾部质点
        ```
    *   **核心细节**: 力并非单点施加，而是通过 `forceDistributeFactor` (默认 0.1) 分配给头部和尾部，确保鱼在游动时姿态自然，而不是像僵尸一样平移。

### C. 鱼竿与鱼线环境 - `FishingRodSimulation.cs`
这是一个巨大的“参数容器”和“状态机”，定义了物理世界的物理常数。
*   **关键参数**:
    *   `RodSpringConstant` (5000f): 鱼竿的硬度。
    *   `LineSpringConstantFishHooked` (1000f): 中鱼时的鱼线弹性（比平时更硬）。
    *   `LineFrictionConstant` (0.002f): 鱼线在空气/水中的阻力。
*   **状态管理**:
    *   维护 `CurrentLineLength`, `FinalLineLength` 等核心变量。
    *   控制 `RodPosition` 和 `RodRotation`，将玩家手势转化为竿的物理位移。
*   **自适应质量**: `AdaptiveLineSegmentMass` 根据鱼的大小调整鱼线节点的质量，防止“小马拉大车”导致物理爆炸。

### D. 软体鱼 (Verlet Soft Body) - `VerletFishBody.cs`
鱼不是刚体，而是由四面体（Tetrahedron）骨架组成的“肉球”。
*   **核心结构**:
    *   **四面体链**: 使用 `AddTetrahedron` 构建鱼身骨架。
    *   **球窝关节 (Ball Joints)**: 使用 `TetrahedronWithBall` 连接各段鱼身，允许弯曲。
*   **波浪驱动**: `WaveDeviationPhase` 和 `WaveDeviationAmp` 驱动关节摆动，模拟鱼的游动姿态（正弦波游动）。
*   **性能优化**: 大量使用 SIMD (`Vector4f`) 进行数学运算。

### E. 混合物理接口 - `MassToRigidBodySpring.cs`
这是连接“软体世界”（鱼/线）和“刚体世界”（Lure/路亚饵/Unity Collider）的胶水。
*   **连接方式**: 一端连着 `MassObject` (Verlet 质点)，一端连着 `RigidBodyObject`。
*   **力学模型**: `SatisfyImpulseConstraintRigidBody`
    *   计算两者距离 -> 对比 `springLength` -> 计算弹力。
    *   **Impulse Transfer**: 将计算出的力同时应用给质点（修改 Velocity）和刚体（修改 Force/Torque）。
*   **特殊处理**: `TopWaterLure` 有专门的 `ImpulseVerticalFactor`，这证实了水面系路亚饵（Topwater）有特殊的物理处理，极易产生之前观察到的“水面弹跳”问题。

## 3. 关键数据流 (Data Flow)

1.  **AI Input**: `FishActionNode` 计算目标力 -> `Mathf.SmoothDamp` -> 写入 `FishAi.Force`.
2.  **Physics Input**: `VerletFishThrustController` 读取 `FishAi.Force` -> 转换为 `WaterMotor4f` (推力).
3.  **Integration**: `SimulationThread` 积分位置 -> 鱼向外游动 -> 拉动连接点.
4.  **Constraint**: `MassToRigidBodySpring` 检测到距离拉长 -> 产生弹簧拉力 -> 拉动 `RigidLure` (路亚饵).
5.  **Environment**: `RigidLure` 碰撞水面 (`Hit`/Collider) -> 产生反作用力/阻力.
6.  **Feedback**: 所受合力通过鱼线传回竿梢 -> `RodBehaviour` 计算张力 -> UI 显示.

## 4. 调试建议 (Debugging Guide)

*   **鱼发疯乱窜？**
    *   检查 `FishActionNode` 中的 `ForceSmoothTimeUp/Down` (0.15s)，平滑时间太短会导致力突变，太长会导致反应迟钝。
*   **鱼线太弹？**
    *   检查 `FishingRodSimulation.cs` 中的 `LineSpringConstant`。
*   **路亚在水面乱飞？**
    *   检查 `MassToRigidBodySpring.cs` 中的 `SatisfyImpulseConstraintRigidBody` 方法，特别是 `impulseThreshold2` 和 `ImpulseVerticalFactor`。
*   **性能卡顿？**
    *   检查 `SimulationThread.cs` 中的 `MaxIterationsPerFrame` (默认 150) 是否过高。

---
*Created by GitHub Copilot on 2026-03-05*
"""

target_file = r"G:\Copilot_OutPut\FishingGame\DeepDive_PhysicsKernel.md"

with open(target_file, "w", encoding="utf-8") as f:
    f.write(content)

print(f"Updated {target_file}")

