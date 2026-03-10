# 钓鱼项目架构分层拆解 (Project Taxonomy)

基于项目使用的中间件（HybridCLR, OxGFrame, Lunaris）与核心玩法代码（RodBehaviour），我们将项目分解为以下五大领域。这种分类方式有助于团队分工和 AI 辅助开发的上下文隔离。

## 1. 核心玩法层 (Core Gameplay Domain)
*这是项目的灵魂，也是目前我们调试的重点。*
*   **物理模拟 (Physics Simulation)**
    *   **模块**: `RodPhysics`, `LineController`, `ReelMechanics`
    *   **功能**: 鱼竿弯曲计算、鱼线张力物理（Mathf.SmoothDamp）、水面阻力模拟。
*   **鱼类 AI (Fish AI)**
    *   **模块**: `FishBehaviorTree`, `StaminaSystem`
    *   **功能**: 鱼的游动状态机（闲置、咬钩、挣扎、疲劳）、鱼的体力消耗与恢复逻辑。
*   **玩家交互 (Player Control)**
    *   **模块**: `InputHandler`, `TouchGestures`
    *   **功能**: 抛竿手势识别、收线力度控制（刺鱼、溜鱼）、震动反馈 (`MoreMountains.NiceVibrations`)。

## 2. 表现与渲染层 (Visuals & Rendering Domain)
*利用 URP 和专业插件实现的视觉表现。*
*   **环境渲染 (Environment)**
    *   **依赖**: `Lunaris.Terrain`, `Lunaris.Rendering`, `MightyTerrain`
    *   **功能**: 高性能水体渲染（反射/折射）、动态天气系统、体积雾 (`urpvolumetricfog`)。
*   **视觉特效 (VFX)**
    *   **依赖**: `Coffee.UIParticle`, `AVProVideo`
    *   **功能**: 水花飞溅粒子、全屏视频特效（中鱼大招）、SSR (屏幕空间反射)。
*   **UI 动效 (UI Motion)**
    *   **依赖**: `DOTween`, `Coffee.SoftMask`
    *   **功能**: 张力条的动态伸缩、获得物品的弹窗动画。

## 3. 系统架构层 (Infrastructure Domain)
*项目的底层骨架，决定了稳定性和扩展性。*
*   **热更新与代码执行 (Hot Update)**
    *   **依赖**: `HybridCLR` (华山版)
    *   **功能**: C# 代码热修复、DLL 补充元数据管理（AOT/Interpreter）。
*   **框架核心 (Framework Core)**
    *   **依赖**: `OxGFrame`
    *   **功能**: 消息中心 (EventCenter)、资源加载 (AssetLoader)、UI 栈管理。
*   **网络通信 (Networking)**
    *   **依赖**: `BestHTTP`
    *   **功能**: HTTP/WebSocket 通信、断线重连、Protobuf 协议处理。

## 4. 外围系统层 (Meta Game Domain)
*玩家成长的数值循环。*
*   **经济与库存 (Economy & Inventory)**
    *   **模块**: `InventorySystem`, `EquipmentManager`
    *   **功能**: 鱼竿/鱼饵/鱼线的属性数值管理、背包系统、商店逻辑。
*   **数据持久化 (Persistence)**
    *   **依赖**: `Gilzoide.SqliteNet`
    *   **功能**: 本地数据库存储（玩家存档、图鉴记录）。

## 5. 平台与安全层 (Platform & Ops Domain)
*上线运营的保障。*
*   **发布支持 (Distribution)**
    *   **依赖**: `Google.Play`, `AppleAuth`
    *   **功能**: 渠道登录、内购 (IAP)、Android App Bundle (AAB) 分包。
*   **安全与监控 (Security & Monitor)**
    *   **依赖**: `OPS.Obfuscator`, `BuglyPlugins`, `SRDebugger`
    *   **功能**: 代码混淆（防止反编译）、崩溃日志上报、真机运行时控制台。

---

## AI 协作建议 (AI Collaboration Strategy)

在后续开发中，请根据修改涉及的层级，调整给 AI 的 Prompt：

1.  **修改物理手感时**:
    *   **关键词**: "Core Gameplay", "RodPhysics", "Hysteresis"
    *   **关注点**: 数值平滑、帧率独立性、手感反馈。

2.  **修改界面相关时**:
    *   **关键词**: "UI", "DOTween", "OxGFrame"
    *   **关注点**: 异步加载、内存泄漏、动画流畅度。

3.  **修改底层配置时**:
    *   **关键词**: "HybridCLR", "Obfuscator", "Build Pipeline"
    *   **关注点**: 编译错误、AOT 泛型问题、包体大小。

