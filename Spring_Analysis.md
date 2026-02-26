# Spring 系统深度分析

## 13.1 三层弹簧系统架构

QHPhysics 实现了三层递进式的弹簧系统，用于模拟钓鱼场景中不同的物理特性：

| 层级 | 类名 | 求解策略 | 应用场景 | 特点 |
|------|------|---------|---------|------|
| **Level 1** | **Spring** | 脉冲约束（速度基） | 主约束求解 | 高精度，高复杂度 |
| **Level 2** | **VerletSpring** | 位置约束（PBD） | 辅助迭代 | 快速简洁，收敛快 |
| **Level 3** | **TetrahedronTorsionSpring** | 几何约束+力矩 | 弯曲/扭转 | 专用约束，精准控制 |

### Spring 类的核心职责

`Spring` 继承 `ConnectionBase` 并实现 `IImpulseConstraint` 接口，是钓线物理的骨干。

**关键成员变量：**

```csharp
protected Single springConstant;           // 弹簧刚度 (k值)
protected Single springLength;             // 当前长度
protected Single targetSpringLength;       // 目标长度
protected Single tension;                  // 当前张力
protected Boolean freezeSatisfyImpulse;    // 冻结求解标志（新功能）
protected Single impulseThreshold2;        // 张力阈值
protected Single impulseVerletMax2;        // Verlet最大张力
protected Vector4f affectMass1Factor;      // Mass1 影响因子（0~1）
protected Vector4f affectMass2Factor;      // Mass2 影响因子（0~1）
```

**求解流程（SatisfyImpulseConstraint）：**

```
1. 计算位置偏差
   posOffset = mass2.Position - mass1.Position
   posOffsetLen = |posOffset|

2. 检查应该拉伸的条件
   if (isRepulsive || posOffsetLen > springLength):
     计算伸长量和方向
     stretch = posOffsetLen - springLength
     direction = posOffset / posOffsetLen

3. 计算脉冲张力
   effectiveInvMass = invMass1 + invMass2
   tension = stretch * effectiveInvMass * invTimeStep

4. 应用速度修正（不直接修正位置！）
   velocityDelta = direction * tension

5. 阻尼衰减（沿弹簧轴方向）
   damping = dot(relativeVelocity, direction) * friction
   velocityDelta -= damping

6. 应用到质点（考虑影响因子）
   mass1.velocity += velocityDelta * invMass1 * affectMass1Factor
   mass2.velocity += velocityDelta * invMass2 * affectMass2Factor
```

### VerletSpring 类的快速迭代

`VerletSpring` 基于**位置约束**而非速度，一步直接修正位置。这对鱼线链的稳定性至关重要。

**Satisfy() 方法 - 距离约束：**

```csharp
// 防压缩性检查：只在被拉伸时纠正
if (!compressible || currentLenSqr > lengthSqr)
{
    // 标量：控制修正幅度
    scalar = (lengthSqr / (currentLenSqr + lengthSqr) - 0.5f) * invMassSum

    // 直接修正位置（无需等待速度积分）
    mass1.Position -= delta * (scalar * mass1.InvMass)
    mass2.Position += delta * (scalar * mass2.InvMass)
}
```

**Solve() 方法 - 阻尼约束：**

```csharp
// 计算速度投影
relVelProj = dot(mass1.PositionDelta, direction)
           - dot(mass2.PositionDelta, direction)

// 沿弹簧轴应用摩擦
damping = direction * relVelProj * friction

mass1.Position -= damping
mass2.Position += damping
```

**与 Spring 类的互补：**
- Spring：速度级，精度高，但需要等待1~2步才能修正位置
- VerletSpring：位置级，立即修正，但只在有条件执行时生效

### TetrahedronTorsionSpring 类 - 复杂约束

用于模拟钓竿/支撑点的弯曲与扭转。采用四面体 + 旋转矩阵的设计。

**结构：** 1个中心点(Mass1) + 3个周围控制点(Tetrahedron[1~3])

**Satisfy() - 长度约束：**
保证中心点到第一个控制点的距离不超过目标长度。

**Solve() - 复杂的几何纠正：**

```
Step 1: 计算四面体重心和法向量
  centroid = (T[1] + T[2] + T[3]) / 3
  axis = (T[0] - centroid).Normalized()

Step 2: 扭转角度计算
  角度 = Asin(dot(axis, cross(prevAxis, curAxis)))
  torsion = Kahan求和(角度差) // 防浮点误差累积

Step 3: 扭转力矩应用
  torque = -torsion * TorsionStiffness
  for each T[i]:
    force = cross(T[i] - centroid, axis) * torque
    T[i].ApplyForce(force)

Step 4: 弯曲修正（使用Quaternion.Slerp）
  quaternion = LookRotation(向前方向, 竿轴向)
  quaternion = Slerp(cur, target, BendStiffness)
  重新投影所有T[i]到新方向

Step 5: 能量衰减
  阻尼沿弹簧轴和弯曲轴分别应用
```

## 13.2 冻结机制（FreezeSatisfyImpulse）

最近添加的功能（commit bf76f74），允许运行时动态禁用约束求解。

**设计目的：**

```csharp
public Boolean FreezeSatisfyImpulse {
    get {
        return freezeSatisfyImpulse;
    }
    set {
        freezeSatisfyImpulse = value;
        ConnectionNeedSyncMark();
    }
}
```

**在 Solve() 中的应用：**

```csharp
public override void Solve() {
    // 线性插值目标长度
    if (!Mathf.Approximately(targetSpringLength, springLength)) {
        springLength = Mathf.Lerp(oldSpringLength, targetSpringLength,
                                 frameIterationProgress);
    }

    // 【新增】冻结标志检查
    if (freezeSatisfyImpulse) {
        return;  // 保留结构，但不求解约束
    }

    // 继续执行约束求解
    tension = SatisfyImpulseConstraintMass(this, Mass1, Mass2,
                                          springLength, frictionConstant4f,
                                          ImpulseThreshold2);
}
```

**应用场景：**

1. **动画插值时期** - 在特定动画帧锁定弹簧，防止物理干扰
2. **特殊交互** - 如鱼线放松、缠绕等特殊状态
3. **性能优化** - 在低性能设备上选择性禁用非关键约束
4. **调试** - 快速关闭某条弹簧以观察其他物理行为

## 13.3 张力与边界条件

### 张力计算

Spring 类不使用 Hooke 定律（F = -kx），而是通过脉冲约束间接实现：

```
张力 = (currentLength - restLength) * effectiveInvMass * timeStepInv
```

这种设计的优势：
- 与 Verlet 积分原生兼容（不需要速度）
- 与高频(2500Hz)迭代配套，单步修正量自动缩放
- 不受 springConstant 的数值爆炸影响（阈值+摩擦天然稳定）

### 三重阈值保护

| 参数 | 作用 | 典型值 |
|------|------|--------|
| `impulseThreshold2` | 低张力时免除摩擦计算（防抖动） | 0.0 |
| `impulseVerletMax2` | Verlet 质点张力上限（防爆炸） | 100.0 |
| `affectMass1Factor` / `affectMass2Factor` | 软约束因子（0~1 灵活衰减） | 1.0 |

## 13.4 链式弹簧与累积误差

钓线模型为多段弹簧串联：

```
RodTip --spring0-- M1 --spring1-- M2 --spring2-- ... --springN-- Hook
```

**单遍求解的问题：**

单遍扫描从 spring0 到 springN，每条弹簧只求解一次：
- spring0：拉 M1 向 RodTip
- spring1：拉 M1 向 M2，拉 M2 向前（部分抵消 spring0 的校正）
- 链越长，末端累积误差越大

与主流引擎(PGS/TGS)的区别：主流引擎在一个 substep 内迭代多次(4~20次)，QHPhysics 只做 1 遍，靠 2500Hz 高频弥补。

## 13.5 与 Hooke 定律的关系

**常见误解：** "Spring 中的 springConstant 就是 k，遵循 F = -kx"

**实际：** Spring 并**不使用** springConstant 进行张力计算！

张力完全由 effectiveInvMass 和 timeStep 推导，与 springConstant 无关。

**springConstant 的实际用途：** 计算平衡长度

**为什么 Spring 不用 Hooke？**
1. 鱼线应该是 **不可拉伸约束**（近似无限刚度）
2. 用约束求解器比显式力计算更稳定（天然数值阻尼）
3. 与 Verlet 积分框架原生兼容

## 13.6 性能特性

### SIMD 向量化

```csharp
protected Vector4f massesInvDenom;  // 用 Vector4f 存储单个值
protected Vector4f frictionConstant4f;
protected Vector4f affectMass1Factor;

// 批量计算时利用 SIMD
mass1.Velocity4f += velocityDelta * mass1.InvMassValue4f * affectMass1Factor;
```

这样做的优势：
- 4 个单精度浮点并行计算（理论 4x 加速）
- 统一的向量接口，便于批处理

## 13.7 缺陷与改进方向

### 当前限制

1. **链式累积误差** - 鱼线超长时可见拉伸
   - 改进方案：在一个 substep 内迭代 2~3 遍求解

2. **单向约束不完整** - affectFactor 可以软化但不能实现完全单向
   - 改进方案：引入显式单向标志

3. **冻结机制粒度** - 只能冻结整条弹簧
   - 改进方案：支持时间线性衰减

4. **静摩擦缺失** - 只有动摩擦
   - 改进方案：在低速/静止时引入静摩擦阈值

## 13.8 调试与可视化建议

### 关键参数检查清单

```csharp
Debug.Log($"Spring {id}:");
Debug.Log($"  Length: {CurrentSpringLength:F3} / {SpringLength:F3}");
Debug.Log($"  Tension: {Tension:F3}");
Debug.Log($"  Frozen: {FreezeSatisfyImpulse}");
Debug.Log($"  Friction: {FrictionConstant:F3}");
Debug.Log($"  ThresholdApplied: {Tension >= ImpulseThreshold2}");
```

## 13.9 总结：Spring 系统的设计哲学

| 维度 | 设计选择 | 理由 |
|------|---------|------|
| **求解方式** | 脉冲约束，非 Hooke | 鱼线约束，需要位置精度 |
| **迭代策略** | 单遍 + 高频 | 实时性 vs 精度的平衡 |
| **稳定性** | 多重阈值截断 | 防数值爆炸，保证可玩性 |
| **灵活性** | 冻结/影响因子/阈值 | 支持丰富的交互场景 |
| **性能** | SIMD 批处理 + 轮转 | 2500Hz 高频下的可承受成本 |
| **兼容性** | 与 VerletMass/Bend 混用 | 钓鱼场景的综合需求 |

Spring 系统是 QHPhysics **最核心的创新**。它不追求通用性或学术正确性，而是为钓鱼手感深度定制，这正是其强大之处——也是最大的局限。
