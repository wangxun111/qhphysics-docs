# PhysX 物理引擎深度分析

## 一、项目概述

PhysX 是由 NVIDIA 开发的通用物理引擎，广泛应用于游戏、VR/AR、机器人仿真等领域。经过多个版本迭代，现在支持 CPU 和 GPU 加速计算。

**最新版本**: PhysX 5.x
**开发商**: NVIDIA（原 Ageia）
**编程语言**: C++（C# 通过 wrapper 访问）
**授权协议**: NVIDIA 专有许可证 + BSD（某些部分开源）
**集成方式**: Unity / Unreal Engine / 自定义引擎

---

## 二、整体架构

```
┌──────────────────────────────────────────────────┐
│              PhysX Runtime                       │
├─────────────────────┬──────────────────────────┤
│  Rigid Body Solver  │   Soft Body / Cloth      │
│  (刚体动力学)        │   (布料、绳索、液体)     │
├─────────────────────┼──────────────────────────┤
│  Constraint System  │   Joint System           │
│  (约束求解)         │   (20+ 关节类型)        │
├─────────────────────┼──────────────────────────┤
│  Collision Engine   │   Continuous Collision  │
│  (碰撞检测)         │   Detection (CCD)       │
├─────────────────────┼──────────────────────────┤
│  Broad Phase (BVH)  │   Narrow Phase (GJK)    │
│  (空间加速)         │   (精确碰撞测试)       │
├──────────────────────────────────────────────────┤
│         Parallel Task Dispatcher                 │
│         (多线程/GPU 任务调度)                    │
├──────────────────────────────────────────────────┤
│         Memory Manager & Allocators              │
│         (自定义内存管理)                        │
└──────────────────────────────────────────────────┘
```

---

## 三、核心模块详解

### 3.1 刚体动力学 (Rigid Body Dynamics)

**特点**：
- 完整的刚体物理模拟
- 支持静态、动态、运动学三种刚体类型
- 自动睡眠机制（休眠优化）
- 角速度、扭矩、惯性张量完整计算

**主要类**：
```
PxRigidActor
├── PxRigidStatic      // 静态物体（地形、建筑物）
└── PxRigidDynamic     // 动态物体（可移动物体）
    ├── 线性速度 (linearVelocity)
    ├── 角速度 (angularVelocity)
    └── 质量属性 (mass, inertia)
```

### 3.2 约束系统 (Constraint Solver)

**求解器类型**：
- **TGS (Temporal Gauss-Seidel)**: 新一代求解器，更精确
- **PGS (Projected Gauss-Seidel)**: 经典求解器，更快

**特点**：
- 每个子步内迭代 4~8 次
- 支持软约束（compliance）和约束阻尼
- 准确的力反馈

### 3.3 关节系统 (Joint System)

PhysX 支持 20+ 种关节类型：

| 关节类型 | 用途 | 自由度 |
|---------|------|--------|
| Fixed Joint | 固定连接 | 0 DOF |
| Spherical Joint | 球形关节 | 3 DOF |
| Revolute Joint | 旋转关节 | 1 DOF |
| Prismatic Joint | 滑动关节 | 1 DOF |
| Distance Joint | 距离约束 | 保持距离 |
| D6 Joint | 通用关节 | 6 DOF |
| Gear Joint | 齿轮关节 | 传动 |
| Pulley Joint | 滑轮关节 | 传动 |

### 3.4 碰撞检测 (Collision Engine)

**宽相阶段 (Broad Phase)**：
```
BVH (Bounding Volume Hierarchy)
├── 动态 BVH 树
├── O(n log n) 复杂度
└── 自动重构优化
```

**窄相阶段 (Narrow Phase)**：
```
GJK + EPA 算法
├── 凸包碰撞 (Convex)
├── 网格碰撞 (TriangleMesh)
├── 高度场碰撞 (HeightField)
└── 胶囊体、球体、盒子等
```

### 3.5 布料与柔体 (Cloth & SoftBody)

**布料特点**：
- Verlet 位置积分
- 布料-刚体碰撞
- 风力、重力、阻力

**柔体特点**：
- 四面体网格
- 体积保持约束
- 实时形变

### 3.6 连续碰撞检测 (CCD)

**机制**：
- Speculative CCD：预测碰撞
- Sweep CCD：轨迹扫描
- 防止隧道效应

---

## 四、核心算法

### 4.1 仿真主循环

```
PhysX.simulate(deltaTime):
  1. 更新运动物体位置和速度
  2. 宽相碰撞检测 (BVH)
  3. 窄相碰撞检测 (GJK/EPA)
  4. 碰撞缓冲生成
  5. 求解所有约束 (TGS/PGS 迭代)
  6. 更新速度和位置
  7. 生成碰撞回调事件
```

### 4.2 半隐式欧拉积分

```csharp
// 速度更新
v_new = v_old + (F / m) * dt + g * dt

// 位置更新
x_new = x_old + v_new * dt

// 旋转更新
ω_new = ω_old + (τ / I) * dt
q_new = q_old + 0.5 * ω_new * q_old * dt
```

**优势**：
- 相对稳定
- 支持大时间步
- 易于调整

### 4.3 TGS 约束求解

```
for each iteration (4~8 times):
    for each constraint:
        计算约束违反量
        计算冲量
        应用到两端刚体
```

**关键特性**：
- 支持约束优先级
- 软约束（Compliance）
- 约束阻尼（Damping）
- 准确的力反馈

### 4.4 GJK 碰撞检测

**步骤**：
```
1. 初始化单纯形（Simplex）
2. 迭代向支撑点方向移动
3. 检查原点是否在单纯形内
4. 如果碰撞，运行 EPA 计算碰撞深度
```

**优势**：
- 处理任意凸形状
- 数值稳定
- 快速收敛

---

## 五、性能特性

### 5.1 多线程架构

```
主线程              物理线程 (WorkQueue)
   ↓                   ↓
simulate() ────→ TaskDispatcher
                   ↓
              ┌────┼────┐
              ↓    ↓    ↓
           任务1  任务2  任务3 (并行)
              └────┼────┘
                   ↓
              取回结果
```

**并行化粒度**：
- 刚体更新（batch）
- 碰撞对处理（并行）
- 约束迭代（数据并行）

### 5.2 GPU 加速

PhysX 5.x 支持 GPU 卸载：
```
CPU: 碰撞检测
GPU: 约束求解、位置积分

数据流：CPU ← GPU (双向传输)
```

### 5.3 内存模型

```
物理场景 (Scene)
├── 刚体列表 (Preallocated)
├── 约束缓冲 (Dynamic)
├── 碰撞对缓冲 (Dynamic)
└── 接触缓冲 (Frame-local)
```

**特点**：
- 自定义内存分配器
- 内存预分配
- 无运行时分配（可选）

---

## 六、高级特性

### 6.1 车辆物理 (Vehicle SDK)

**包含**：
- 轮胎模型（轮胎力计算）
- 悬挂系统
- 动力系统
- 转向系统
- 防滑控制 (ABS, ESC)

### 6.2 破碎系统 (Destruction)

**支持**：
- 实时网格破碎
- 凸壳分解
- 碎片刚体生成

### 6.3 液体和粒子

**FLIP/PIC 粒子流体**：
- 流体仿真
- 粒子碰撞
- 密度约束

---

## 七、与 QHPhysics 对比分析

### 7.1 设计理念对比

| 维度 | PhysX | QHPhysics |
|------|-------|-----------|
| **目标** | 通用物理引擎 | 钓鱼游戏专用 |
| **优先级** | 物理准确性 | 实时性和手感 |
| **复杂度** | 高（功能全面） | 低（简洁专用） |
| **扩展性** | 高（支持各种场景） | 低（为钓鱼定制） |

### 7.2 核心算法对比

| 特性 | PhysX 5.x | QHPhysics |
|------|-----------|-----------|
| **积分方法** | 半隐式欧拉 | Verlet + 冲量混合 |
| **时间步长** | 1/60s ~ 1/240s | 固定 0.0004s (2500Hz) |
| **约束迭代** | 4~8 次/子步 | 1 次 + 高频 |
| **碰撞检测** | GJK + EPA | 点-几何体测试 |
| **宽相** | BVH | 无（暴力） |
| **CCD** | Speculative + Sweep | 无 |

### 7.3 功能完整性对比

| 功能 | PhysX | QHPhysics |
|------|-------|-----------|
| **刚体** | ✅ 完整 | ✅ 基础 |
| **关节** | ✅ 20+ 种 | ✅ 4 种 |
| **约束** | ✅ 全类型 | ✅ 专用 |
| **布料** | ✅ 有 | ❌ 无 |
| **粒子流体** | ✅ 有 | ❌ 无 |
| **车辆** | ✅ 完整 SDK | ❌ 无 |
| **破碎** | ✅ 有 | ❌ 无 |
| **绳索** | ❌ 无 | ✅ 优秀 |
| **鱼体模拟** | ❌ 无 | ✅ 专用 |
| **水物理** | ❌ 无 | ✅ 内置 |

### 7.4 性能对比

| 指标 | PhysX | QHPhysics |
|------|-------|-----------|
| **线程模型** | 多线程 + GPU | 独立线程单核 |
| **扩展性** | 数万刚体 | 数百质点 |
| **内存占用** | 高（功能多） | 低（专用） |
| **学习曲线** | 陡峭 | 平缓 |

---

## 八、使用场景

### PhysX 适用场景

```
✅ AAA 游戏（刚体为主）
✅ VR/AR 应用
✅ 机器人仿真
✅ 车辆物理
✅ 布料/破碎
✅ 大规模场景
```

### QHPhysics 适用场景

```
✅ 钓鱼游戏
✅ 鱼体动画
✅ 鱼线模拟
✅ 实时手感调优
✅ 移动端轻量级物理
```

---

## 九、集成与使用

### 9.1 基本使用流程

```csharp
// 创建物理场景
PxPhysics* physics = PxCreatePhysics(...);
PxScene* scene = physics->createScene(...);

// 创建刚体
PxRigidDynamic* box = physics->createRigidDynamic(
    PxTransform(position)
);
PxBoxGeometry geometry(halfExtents);
box->attachShape(PxRigidActorExt::createExclusiveShape(
    *box, geometry, material
));
scene->addActor(*box);

// 模拟
scene->simulate(1.0f / 60.0f);  // 16ms 步长
scene->fetchResults(true);

// 读取结果
PxTransform transform = box->getGlobalPose();
```

### 9.2 约束示例

```csharp
// 创建固定关节
PxFixedJoint* joint = PxFixedJointCreate(
    physics,
    actor1, PxTransform(local1),
    actor2, PxTransform(local2)
);

// 创建旋转关节
PxRevoluteJoint* revolute = PxRevoluteJointCreate(
    physics,
    actor1, PxTransform(PxVec3(0, 0, 0),
                        PxQuat(PxPiDivTwo, PxVec3(0, 1, 0))),
    actor2, PxTransform(local2)
);
revolute->setDriveVelocity(speed);
revolute->setDriveForceLimit(maxForce);
```

---

## 十、优缺点分析

### PhysX 优势

1. **功能完整** ✅
   - 刚体、布料、粒子、破碎一应俱全
   - 支持复杂的多物体系统

2. **算法精确** ✅
   - TGS 迭代求解保证准确性
   - GJK + EPA 碰撞检测稳定

3. **性能优秀** ✅
   - 多线程并行化
   - GPU 加速支持
   - BVH 宽相加速

4. **业界标准** ✅
   - Unity / Unreal 官方支持
   - 文档齐全
   - 社区活跃

### PhysX 劣势

1. **复杂度高** ❌
   - 学习曲线陡峭
   - 参数众多难以调优
   - 集成成本大

2. **不适合特定领域** ❌
   - 无绳索/布料原生支持（相对于 QHPhysics）
   - 无内置水物理
   - 不适合高频迭代（2500Hz）

3. **资源占用** ❌
   - 内存占用大
   - CPU/GPU 成本高

### QHPhysics 优势

1. **专用定制** ✅
   - 为钓鱼场景优化
   - 参数调优容易
   - 手感可控

2. **轻量级** ✅
   - 代码量少
   - 内存占用低
   - 移动端友好

3. **高频迭代** ✅
   - 2500Hz 固定步长
   - 精确的鱼线模拟
   - 实时交互性好

### QHPhysics 劣势

1. **功能有限** ❌
   - 不支持布料、粒子、破碎
   - 碰撞系统简陋
   - 不适合通用场景

2. **可扩展性差** ❌
   - 硬编码上限（1024质点）
   - 双轨积分耦合
   - 难以用于其他游戏类型

3. **算法限制** ❌
   - 单遍约束求解
   - 无迭代收敛
   - 链式弹簧累积误差

---

## 十一、选择建议

### 选择 PhysX 当：

```
✅ 开发通用游戏（不限于钓鱼）
✅ 需要完整的物理模拟
✅ 项目规模大，有充足人力
✅ 目标平台支持（Unity/Unreal）
✅ 需要高度精确的物理
```

### 选择 QHPhysics 当：

```
✅ 专门开发钓鱼游戏
✅ 需要轻量级引擎
✅ 强调实时手感调优
✅ 目标移动端
✅ 希望完全定制控制
```

### 混合方案：

```
✅ PhysX 用于场景刚体（地形、建筑）
✅ QHPhysics 用于钓鱼部分（鱼线、鱼体）
✅ 两个系统并行运行，独立交互
```

---

## 十二、发展趋势

### PhysX 的未来方向

```
→ GPU 计算比重增加
→ CUDA/OptiX 集成深化
→ 实时 ray tracing 物理
→ AI 辅助参数调优
→ 云端物理仿真
```

### QHPhysics 的演化可能

```
→ 增加迭代约束求解
→ 扩展约束类型
→ GPU 加速
→ 支持其他柔体
→ 开源社区版本
```

---

## 总结

**PhysX** 和 **QHPhysics** 代表了物理引擎设计的两个极端：
- **PhysX**：通用、准确、复杂、强大
- **QHPhysics**：专用、快速、简洁、灵活

没有绝对的"更好"，只有"更适合"。选择取决于你的具体需求。

对于钓鱼游戏，QHPhysics 已经是**最优方案**。
对于其他游戏类型，PhysX 仍是**业界标准**。

---

**文档版本**: v1.0
**更新时间**: 2026年2月26日
**对标引擎**: PhysX 5.x / QHPhysics 1.x
