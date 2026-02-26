# QHPhysics 物理引擎架构文档

## 一、项目概述

QHPhysics 是一个面向钓鱼游戏的自研物理引擎 Unity 插件，核心目标是高性能模拟鱼线、鱼竿、鱼钩、鱼体等柔性/刚性物体的物理行为。

**技术栈**: C# / .NET Standard 2.1 / Unity
**依赖**: Newtonsoft.Json、Mono.Simd（自带 SIMD 向量库）

---

## 二、整体架构

```
┌─────────────────────────────────────────────┐
│              SimulationSystem               │
│  (主循环: 力 → 弹簧求解 → Verlet约束满足)     │
├──────────┬──────────┬───────────────────────┤
│ Mass系统  │ 约束系统  │  碰撞系统             │
│ MassObject│ Spring   │  SphereCollider       │
│ VerletMass│ Bend     │  BoxCollider          │
│ RigidBody │ Magnet   │  CapsuleCollider      │
│           │ Kinematic│  PlaneSudokuCollider   │
├──────────┴──────────┴───────────────────────┤
│  PhysicsObject (高层容器: 鱼线/鱼竿/鱼等)     │
├─────────────────────────────────────────────┤
│  Math工具: BezierSpline / VerticalParabola   │
├─────────────────────────────────────────────┤
│  Mono.Simd (SIMD向量加速)                    │
└─────────────────────────────────────────────┘
```

### 设计模式

| 模式 | 应用 |
|------|------|
| 观察者模式 | `IPhysicsEngineListener` 解耦物理与渲染/游戏逻辑 |
| 对象池 | 预分配定长数组（Mass 1024、Connection 1024） |
| 工厂模式 | `SplineFactory` 创建样条曲线 |
| 混合积分 | Verlet（绳索/鱼体）+ 冲量约束（弹簧）+ 力学积分（通用质点） |

---

## 三、核心模块详解

### 3.1 质点系统 (Mass)

所有物理实体的基本单元是**质点 (MassObject)**。

| 类 | 用途 |
|----|------|
| `MassObject` | 基础质点：位置、速度、质量、半径、摩擦、浮力 |
| `VerletMass` | Verlet积分质点：通过位置差隐式表达速度，数值更稳定 |
| `RigidBodyObject` | 刚体：增加旋转、角速度、惯性张量 |

**质点类型枚举 (EMassObjectType)**:
Rod / Line / Leader / Lure / Bobber / Sinker / Hook / Fish / Feeder / Swivel 等 17 种钓鱼相关类型。

### 3.2 约束系统 (Connection)

约束连接两个质点，维持物理关系。

| 约束类 | 算法 | 用途 |
|--------|------|------|
| `Spring` | 冲量求解 (Hooke定律) | 弹性连接，支持冻结 |
| `VerletSpring` | 位置约束满足 | Verlet绳索段 |
| `Bend` | 旋转弯曲约束 | 鱼竿弯曲刚度 |
| `VerletBend` | Slerp旋转插值 | Verlet弯曲 |
| `KinematicConnection` | Bezier样条路径 | 动画驱动运动 |
| `KinematicVerticalParabola` | 抛物线轨迹 | 抛竿/抛投 |
| `Magnet` | 距离衰减吸引力 | 磁力吸附 |
| `Tetrahedron*` | 四面体约束 | 体积保持/扭转弹簧 |

### 3.3 碰撞系统 (Collider)

| 碰撞体 | 说明 |
|---------|------|
| `SphereCollider` | 球体碰撞，包装 Unity SphereCollider |
| `BoxCollider` | 盒体碰撞 |
| `CapsuleCollider` | 胶囊体碰撞 |
| `PlaneSudokuCollider` | 网格化平面碰撞（地形） |

碰撞响应：反弹 + 摩擦 + 粘附（Sticky 密度类型可捕获物体）。

### 3.4 物理对象容器 (PhysicsObject)

`PhysicsObject` 将多个质点和约束组合为一个逻辑实体（鱼线、鱼竿、鱼等）。
`VerletObject` 扩展了便捷方法：`AddMass()` / `AddSpring()` / `AddTetrahedron()` / `AddBallJoint()`。

**对象类型**: Rod / Line / Lure / Bobber / Leader / Hook / Fish / Magnet / Sinker / Plant / Feeder / Leash / RubberStopper

---

## 四、关键算法

### 4.1 仿真主循环

```
SimulationSystem.Update(deltaTime):
  累积时间 → 按固定步长 0.0004s 迭代:
    1. ApplyForcesToMasses()   // 重力、电机力、环境力
    2. Spring.Solve()          // 冲量弹簧求解
    3. SatisfyVerletConstraints() // Verlet约束满足（3周期轮转）
    4. MassObject.Simulate()   // 积分 + 碰撞
```

固定步长 0.0004s（2500Hz）保证数值稳定性。

### 4.2 Verlet 积分

核心思想：用**位置差**代替显式速度，天然稳定。

```
速度(隐式) = Position - PrevPosition
新位置 = Position + 速度 + 加速度 * dt²
```

使用 **Kahan 求和** 减少浮点累积误差：
```
kahanAccum 追踪舍入误差
Position = Position + (delta - kahanAccum)
kahanAccum = (newPos - oldPos) - delta
```

鱼体 Y 轴特殊处理（沉降效果）：
```
Y = deltaPos.Y + (velocity.Y - deltaPos.Y) * 0.1
```

### 4.3 弹簧约束

**冲量弹簧 (Spring)** — Hooke 定律 + 阻尼：
```
张力 tension = (distance - restLength) * k / dt
冲量 J = tension * direction
v1 += J / m1,  v2 -= J / m2
```
支持 `ImpulseThreshold2`（最小张力阈值）和 `FreezeSatisfyImpulse`（冻结求解）。

**Verlet弹簧 (VerletSpring)** — 位置修正：
```
correction = (lengthSqr / (distSqr + lengthSqr) - 0.5) * invMassSum
mass1.Position += correction * delta
mass2.Position -= correction * delta
```
支持可压缩/不可压缩模式。

### 4.4 弯曲约束 (Bend)

维持三个质点间的角度关系，模拟鱼竿弯曲刚度：
```
F_bend    = bendConstant * (pos - unbentPos)     // 恢复力
F_spring  = springConstant * (dist - restDist)    // 距离保持
F_friction = frictionConstant * (v1 - v2)         // 阻尼
```

VerletBend 使用 **Slerp** 做旋转插值，`displacementStiffness` 控制位移刚度。

### 4.5 水阻力与浮力

当质点 Y < DefaultRadius（水面以下）时：
```
层流阻力: resistance = waterDrag * radiusFactor * invMass * dt
二次阻力: velocity *= 1 / (1 + speed * waterResistance)
```
浮力通过 `EDensityType`（None/Solid/Sticky/Sparse）和 `buoyancy` 参数控制。

### 4.6 碰撞响应

```
反弹: v += -normal * dot(v, normal) * bounceFactor
摩擦: v_tangent *= (1 - frictionFactor)
粘附: EDensityType.Sticky 可捕获物体
```

### 4.7 Bezier 样条曲线

用于 `KinematicConnection` 的平滑动画路径。

**求值** (参数 t ∈ [0,1]):
```
P(t) = Σ C(n,i) * t^i * (1-t)^(n-i) * P_i
```

**导数** (切线/速度):
```
P'(t) = n * Σ C(n-1,i) * t^i * (1-t)^(n-1-i) * (P_{i+1} - P_i)
```

支持**弧长重参数化**：采样建立查找表，将均匀参数映射到弧长参数，实现匀速运动。

**圆柱变换** (TransformByCylinder2): 将 2D 局部坐标沿曲线映射到 3D 空间，使用 Rodrigues 旋转公式。

### 4.8 抛物线轨迹 (VerticalParabola)

模拟抛竿等抛物运动：
```
y = a*x² + b*x  (由起点、终点、峰值高度求解 a, b)
```

支持**侧向偏移**：
```
lateralOffset = tan(lateralAngle) * |y| * 0.3
```
`KinematicVerticalParabola` 通过 `progress = ((time - start) / duration) ^ timePower` 驱动质点沿轨迹运动。

---

## 五、SIMD 优化 (Mono.Simd)

自带 SIMD 向量库，核心类型 `Vector4f`（4 分量 float 向量）。

- 所有 3D 向量运算使用 `Vector4f`（第 4 分量填充）
- 批量处理 4 个质点的运算
- 添加 Padding 质点使数组长度对齐 4 的倍数
- 优化的点积、叉积、Shuffle 操作

---

## 六、关键常量

| 常量 | 值 | 说明 |
|------|-----|------|
| TimeQuant | 0.0004s | 固定仿真步长 (2500Hz) |
| MassesMaxAmount | 1024 | 最大质点数 |
| ConnectionsMaxCount | 1024 | 最大约束数 |
| VerletConstraintsMaxCount | 1024 | 最大Verlet约束数 |
| 重力加速度 | 9.81 | m/s² |
| 水阻力系数 | 10.0 | waterDragConstant |
| 空气阻力 | 0.001 | airDragConstant |
| 默认速度上限 | 20 m/s | 无鱼时 |
| 鱼体速度上限 | 100 m/s | 有鱼时 |
| 默认质点半径 | 0.0125m | 也是水面判定阈值 |
| 最大物理对象数 | 128 | PhysicsObject |

---

## 七、外部集成接口

### 使用流程

```csharp
// 1. 创建仿真系统
var sim = new SimulationSystem();

// 2. 注册监听器（接收物理状态变化回调）
sim.SetListener(myListener); // IPhysicsEngineListener

// 3. 构建物理对象（质点 + 约束）
var line = new VerletObject();
line.AddMass(position, mass, radius);
line.AddSpring(mass1, mass2, stiffness, friction);
sim.AddPhysicsObject(line);

// 4. 每帧更新
sim.Update(Time.deltaTime);

// 5. 通过 IPhysicsEngineListener 回调同步渲染
```

### IPhysicsEngineListener 关键回调

- 质点生命周期: `CreateAMass()` / `DestroyAMass()`
- 属性变化: 位置、旋转、速度、力、质量、阻力、浮力等
- 状态变化: Kinematic、Freeze、碰撞类型
- 钓鱼专用: `ChangeFishIsHooked()` / `SetTopWaterStrikeFlg()` / `SetTopWaterLiftValue()`

---

## 八、目录结构速查

```
QHForUnity/
├── Mono.Simd/              # SIMD向量数学库
├── Third/                  # 第三方依赖 (Unity DLL, Json)
└── QHPhysics/
    ├── External/           # 公开接口 (IMassObject)
    └── Internal/
        ├── Simulation/     # 仿真主循环
        ├── Mass/           # 质点系统
        ├── Connection/     # 约束系统 (弹簧/弯曲/运动学/磁力/四面体)
        ├── Collider/       # 碰撞检测
        ├── Chunk/          # 物理对象容器
        └── Math/           # 数学工具 (Bezier/抛物线/向量)
```

---

## 九、框架难点分析

### 难点 1：双轨积分体系的协同

框架同时运行两套物理求解，这是最核心的复杂度来源：

- **Verlet 积分**（位置驱动）— 用于绳索/鱼体，天然稳定但不易施加精确力
- **冲量弹簧**（速度驱动）— 用于弹性连接，灵活但容易数值爆炸

两者在同一个仿真循环里交替执行，共享质点数据。理解它们如何分工、如何在同一个质点上叠加效果而不冲突，是掌握框架的第一道坎。

### 难点 2：约束求解的迭代收敛

Verlet 约束满足采用 **3 周期轮转**策略（每 3 帧换一批约束求解），避免过约束导致震荡。这意味着约束不是每帧都精确满足的，而是逐步收敛。调参时如果不理解这个机制，容易出现"弹簧太软"或"抖动"的问题。

### 难点 3：2500Hz 固定步长的性能约束

`TimeQuant = 0.0004s`，60fps 下每帧约 42 次物理迭代。设计保证了数值稳定，但也意味着：
- 性能极度敏感，单次迭代的开销被放大 42 倍
- 弹簧刚度、阻尼等参数都基于此步长调校，不能随意修改步长

### 难点 4：钓鱼场景的经验参数

鱼体 Y 轴 0.1 混合因子、`FishData` 对弹簧行为的开关控制、水面判定用质点半径当阈值 — 这些是针对钓鱼手感调出来的经验值，没有通用物理公式可循。修改手感需要理解这些 magic number 背后的意图。

### 难点 5：SIMD 对齐与 Padding 机制

所有向量运算使用 `Vector4f`（4 分量），质点数组需要 Padding 对齐到 4 的倍数。增删质点时必须维护对齐，否则 SIMD 批处理会越界或产生错误结果。

---

## 十、掌握框架的学习主线

建议按以下顺序逐层深入，每层吃透再进入下一层：

```
质点 → 约束 → 仿真循环 → 对象组装 → 外部集成
(沿数据流从底层往上走)
```

### 第一步：理解质点生命周期

从 `MassObject.Simulate()` 入手，搞清一个质点每帧经历的完整流程：
力的累积 → 速度积分 → 位置更新 → 碰撞响应。这是一切的基础。

### 第二步：理解仿真主循环

读 `SimulationSystem` 的 Update 流程，把握四步执行顺序和数据流向：
```
1. ApplyForcesToMasses()        // 力累积
2. Spring.Solve()               // 冲量弹簧求解
3. SatisfyVerletConstraints()   // Verlet约束满足
4. MassObject.Simulate()        // 积分 + 碰撞
```

### 第三步：分别吃透两套弹簧

- 先看 `Spring.SatisfyImpulseConstraint()` — 冲量弹簧怎么算张力、怎么分配给两端质点
- 再看 `VerletSpring.Satisfy()` — 位置修正公式的几何含义：
  ```
  correction = (lengthSqr / (distSqr + lengthSqr) - 0.5)
  ```
  这两个搞明白，约束系统就通了大半。

### 第四步：理解 PhysicsObject 的组装

看 `VerletObject` 怎么用 `AddMass` / `AddSpring` / `AddTetrahedron` 把质点和约束组装成鱼线、鱼竿等实体。这是从算法到业务的桥梁。

### 第五步：IPhysicsEngineListener 回调机制

理解物理层怎么通过观察者模式把状态变化推给外部（渲染/游戏逻辑），这是集成和调试的关键入口。

### 一句话总结

**双轨积分（Verlet + 冲量）的协同是最需要花时间的部分**，理解了它们的分工与数据流，整个框架就豁然开朗。

---

## 十一、冲量约束求解核心计算详解

### 11.1 核心接口与数据结构

```csharp
// 冲量约束接口 — 所有冲量求解器必须实现
public interface IImpulseConstraint {
    void SatisfyImpulseConstraint();
}
```

Spring 类的关键成员变量：

| 变量 | 物理含义 |
|------|----------|
| `springConstant` | 弹簧刚度 k |
| `springLength` | 当前弹簧自然长度（可动画插值） |
| `targetSpringLength` | 目标自然长度 |
| `massesInvDenom` | 约化质量 = 1/(1/m1 + 1/m2) |
| `tension` | 当前张力值 |
| `frictionConstant4f` | 阻尼/摩擦系数 |
| `affectMass1Factor` | mass1 冲量缩放因子 (0~1) |
| `affectMass2Factor` | mass2 冲量缩放因子 (0~1) |
| `impulseThreshold2` | 最小张力阈值（低于此值不施加摩擦） |
| `impulseVerletMax2` | Verlet质点最大张力上限（防爆炸） |
| `freezeSatisfyImpulse` | 冻结开关（跳过求解） |

### 11.2 Solve() 入口流程

```
Spring.Solve():
  ┌─ 帧首次迭代? → 保存 oldSpringLength
  ├─ targetLength ≠ currentLength? → Lerp插值过渡
  ├─ freezeSatisfyImpulse? → 直接返回（冻结）
  ├─ 两端都是Kinematic? → 直接返回（无需求解）
  └─ 调用 SatisfyImpulseConstraintMass() → 核心计算
```

弹簧长度在帧内通过 `Lerp(old, target, progress)` 平滑过渡，避免突变。

### 11.3 核心计算：SatisfyImpulseConstraintMass() 逐步拆解

这是整个冲量系统的核心函数，完整计算流程如下：

#### 第一步：准备约化质量

```
massesInvDenom = 1 / (1/m1 + 1/m2)    // 约化质量
```

物理意义：约化质量决定冲量如何在两端质点间分配。质量越大的一端，分到的速度变化越小。

特殊情况 `isHitch=true`（锚定模式）：
```
massesInvDenom = m1        // mass1视为锚点
mass2.InvMass = 0          // mass2不受冲量
```

#### 第二步：计算预测分离向量

```
v_rel = v1 - v2                                    // 相对速度
d_predicted = (x2 - x1) - v_rel * dt              // 预测分离向量
d_sqr = |d_predicted|²                             // 分离距离平方
```

关键设计：不是用当前位置差，而是**减去相对速度带来的预测位移**。这相当于在计算"如果不施加约束，下一步两个质点会偏离多远"，是一种**预测-校正**策略。

#### 第三步：判断是否需要求解

```
if d_sqr < 1E-12:  跳过（防除零）
if !IsRepulsive && d_sqr ≤ springLength²:  跳过（未拉伸，非排斥弹簧）
```

非排斥弹簧只在**拉伸**时产生力，压缩时不产生力（类似绳索）。
排斥弹簧在任何偏离自然长度时都产生力（类似刚性杆）。

#### 第四步：计算张力（核心公式）

```
d = sqrt(d_sqr)                                    // 实际距离
dir = d_predicted / d                              // 单位方向向量
tension = (d - springLength) * massesInvDenom / dt // 张力
```

公式推导：
```
Hooke定律:  F = k * Δx           (力 = 刚度 × 形变量)
冲量:       J = F * dt           (冲量 = 力 × 时间)
速度变化:   Δv = J / m = F*dt/m  (速度变化 = 冲量/质量)

本框架简化: Δv = (d - L₀) * m_reduced / dt
其中 m_reduced = 1/(1/m1 + 1/m2)
```

注意：这里的 `springConstant` 并未显式出现在张力公式中 — 框架将刚度隐含在约化质量和时间步长中，实际效果等价于**极高刚度弹簧**（约束级别的刚度），每步都试图完全消除形变。

#### 第五步：计算两端冲量

```
Δv1 = dir * tension                                // mass1 的速度冲量
Δv2 = -Δv1                                         // mass2 的速度冲量（反向）
```

如果张力超过阈值 `impulseThreshold2`：
```
Δv2 = -Δv1 + dir * impulseThreshold2              // mass2 冲量被截断
```

这个截断机制的作用：当张力很大时，mass2 只承受阈值以内的冲量，防止小质量物体被弹飞。

#### 第六步：摩擦/阻尼（条件施加）

```
if tension ≥ impulseThreshold2:
    v_rel_new = (v_rel + Δv1 - Δv2)               // 施加冲量后的新相对速度
    f_damping = v_rel_new * massesInvDenom * friction  // 阻尼力
    Δv1 -= f_damping                               // 减少 mass1 冲量
    Δv2 += f_damping                               // 减少 mass2 冲量
```

物理意义：摩擦力与**施加冲量后的相对速度**成正比，方向相反。只有张力超过阈值时才施加，避免低张力时的数值抖动。

#### 第七步：应用冲量到质点

**普通质点 (MassObject)**：
```
v1 += Δv1 * (1/m1) * affectMass1Factor
v2 += Δv2 * (1/m2) * affectMass2Factor
```

**Verlet 质点 (VerletMass)**：
```
if tension > impulseVerletMax2:
    Δv2 += dir * (tension - impulseVerletMax2)     // 超限部分额外补偿

// 速度冲量转位置冲量（Verlet用位置积分）
Δx_prev = -Δv2 * dt * (1/m2) * affectMass2Factor
prevPosition += Δx_prev                            // 修改前一帧位置
```

Verlet 质点的关键：通过修改 `prevPosition` 来间接改变速度（因为 Verlet 速度 = Position - PrevPosition）。使用 **Kahan 求和** 保证精度。

### 11.4 完整计算流程图

```
SatisfyImpulseConstraintMass(spring, mass1, mass2, springLength, friction, threshold)
│
├─ 1. 准备约化质量 massesInvDenom = 1/(1/m1 + 1/m2)
│
├─ 2. v_rel = v1 - v2
│     d_predicted = (x2 - x1) - v_rel * dt
│     d_sqr = |d_predicted|²
│
├─ 3. if d_sqr < 1E-12 → 跳过
│     if !repulsive && d_sqr ≤ L₀² → 跳过
│
├─ 4. d = sqrt(d_sqr)
│     dir = d_predicted / d
│     tension = (d - L₀) * massesInvDenom / dt
│     Δv1 = dir * tension
│     Δv2 = -Δv1 (或截断至 threshold)
│
├─ 5. if tension ≥ threshold:
│       damping = (v_rel + Δv1 - Δv2) * massesInvDenom * friction
│       Δv1 -= damping,  Δv2 += damping
│
└─ 6. 应用冲量:
      mass1.v += Δv1 / m1 * factor1
      mass2 是 Verlet? → prevPos -= Δv2 * dt / m2 * factor2
      mass2 是普通?   → mass2.v += Δv2 / m2 * factor2
```

### 11.5 公式汇总表

| 公式 | 表达式 | 物理意义 |
|------|--------|----------|
| 约化质量 | `M = 1/(1/m1 + 1/m2)` | 双体系统等效质量 |
| 预测分离 | `d = (x2-x1) - (v1-v2)*dt` | 下一步的预测距离 |
| 张力 | `T = (|d| - L₀) * M / dt` | 约束违反量转速度冲量 |
| 阻尼 | `f = v_rel_new * M * friction` | 耗散相对运动能量 |
| 普通冲量 | `Δv = T * dir / m` | 速度修正 |
| Verlet冲量 | `Δx_prev = -T * dir * dt / m` | 位置修正（等效速度修正） |

### 11.6 关键设计决策总结

1. **预测-校正策略**：用 `(x2-x1) - v_rel*dt` 而非简单的 `x2-x1`，提前考虑速度趋势，减少一步延迟，提高响应速度。

2. **Spring 是约束求解器，不是弹簧力模拟器**：张力公式中没有弹簧刚度 k（`springConstant` 不参与张力计算），每步直接用约化质量和时间步长计算冲量，目标是"尽量消除形变"。刚度由迭代频率（2500Hz）隐式决定 — 迭代越多，约束越硬。这对鱼线是正确的：张紧状态下鱼线近似不可拉伸。

3. **springConstant 的实际用途**：
   - `EquilibrantLength(force)` — 计算给定外力下的平衡长度：`L₀ + F/k`
   - `CalculatePotentialEnergy()` — 计算弹性势能：`k * Δx²`
   - 在 `Bend` 类中用于真正的 Hooke 定律力计算：`F = k * Δx`

4. **两种"弹簧"语义**：
   - `Spring` = **距离约束**（绳索/鱼线），刚度无限，靠高频迭代收敛
   - `Bend` = **真正的弹簧力**（鱼竿弯曲），刚度由 springConstant 控制
   弹性行为在 Spring 中通过 `frictionConstant`（阻尼）和 `impulseThreshold2`（张力阈值截断）间接表现。

5. **阈值截断双保险**：
   - `impulseThreshold2`：低张力时不施加摩擦（防抖动），高张力时截断 mass2 冲量（防弹飞）
   - `impulseVerletMax2`：Verlet 质点额外的张力上限（防数值爆炸）

6. **Verlet 兼容层**：冲量弹簧本质是速度级求解器，但 Verlet 质点没有显式速度。通过 `Δv * dt → Δx_prev` 的转换桥接两套体系，是双轨积分协同的关键接口。

7. **affectFactor 软控制**：通过 0~1 的缩放因子，可以实现单向约束（只拉一端）、软约束（半强度）等灵活配置，无需修改求解器逻辑。

8. **冻结机制 (FreezeSatisfyImpulse)**：允许运行时动态开关约束求解，用于钓鱼场景中鱼线松弛、收线等状态切换。

### 11.7 为什么鱼线会被拉伸（约束不完美收敛分析）

尽管 Spring 的设计目标是"尽量消除形变"，但实际运行中鱼线**可以被拉伸**，极端情况下甚至拉得很长。这不是 bug，而是多个机制叠加导致的结构性限制：

#### 根因一：冲量修正速度，而非直接修正位置（最根本原因）

```
张力 → 速度冲量 Δv → 下一步才积分到位置 Δx = Δv * dt
```

冲量求解修改的是**速度**，位置要等下一个 `Simulate()` 才更新。形变校正天然滞后 1~2 个子步。如果用的是位置直接修正（如 Verlet 约束那样），一步就能拉回，但冲量弹簧做不到。

#### 根因二：速度钳制截断了校正能力

```
MassObject:  CurrentVelocityLimit = 20 m/s (无鱼) / 100 m/s (有鱼)
VerletMass:  currentVelocityDeltaLimit = limit * dt = 100 * 0.0004 = 0.04m/步
```

假设鱼线被拉伸 0.1m，需要的校正速度为 `0.1 / 0.0004 = 250 m/s`，远超上限 100 m/s。速度被钳制后，每步最多校正 0.04m，剩余 0.06m 留到下一步。**鱼拉得越猛，形变越大，钳制效应越严重。**

#### 根因三：链式弹簧单遍求解，相互干扰

鱼线是多段弹簧串联：
```
竿尖 --spring0-- mass1 --spring1-- mass2 --spring2-- ... --springN-- 鱼钩
```

求解顺序是从 spring0 到 springN 单遍扫描：
- spring0 把 mass1 拉向竿尖
- spring1 又把 mass1 拉向 mass2（部分抵消 spring0 的校正）
- 每个弹簧只求解一次，没有迭代细化

**链越长，末端累积误差越大**，这就是为什么鱼线长时拉伸更明显。

#### 根因四：impulseVerletMax2 硬性封顶

```
if tension > impulseVerletMax2 (默认100):
    Verlet质点的冲量被截断
```

当张力 > 100 时，Verlet 质点只能得到 100 单位的校正。鱼挣扎产生的瞬时张力可能远超此值，导致大量校正被丢弃。

#### 根因五：每子步只求解一次，无迭代收敛

```
Operate(dt):
  Solve()      // 所有弹簧求解 1 次
  Simulate()   // 积分 1 次
```

没有像 PBD（Position Based Dynamics）那样在一个子步内迭代多次直到收敛。虽然 2500Hz 的高频弥补了一部分，但大形变时一步一次仍然不够。

#### 各因素影响程度

| 机制 | 影响 | 严重度 |
|------|------|--------|
| 冲量→速度→位置 的间接校正 | 天然滞后 1~2 步 | **关键** |
| 速度钳制 (VelocityLimit) | 大形变时校正被截断 | **关键** |
| 链式弹簧单遍、相互干扰 | 长链末端累积误差 | **高** |
| impulseVerletMax2 封顶 | 高张力时校正不足 | **高** |
| 每子步无迭代收敛 | 大形变需要多帧才能恢复 | **中** |
| affectFactor < 1 | 人为削弱校正 | **中** |
| impulseThreshold2 截断 | mass2 冲量被限制 | **低** |

#### 极端拉伸的典型场景

```
大鱼快速冲刺 → 鱼钩高速远离竿尖
  → 链式弹簧每段都被拉伸
    → 张力极大但被 VelocityLimit 和 VerletMax2 截断
      → 每步只能校正一小部分
        → 多段累积 → 鱼线可见地被拉长
```

#### 设计取舍

这是**稳定性 vs 约束精度**的经典权衡：
- 去掉速度钳制 → 约束更准，但数值可能爆炸
- 增加迭代次数 → 收敛更好，但性能成倍增加
- 改用位置直接修正 → 一步到位，但与冲量体系不兼容

当前设计选择了**牺牲约束精度换取数值稳定性和性能**，对大多数钓鱼场景可以接受，但在极端情况下鱼线确实会明显拉伸。
