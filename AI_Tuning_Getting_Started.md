# AI参数调优入门教程：从零到一的完整指南

## 零、为什么需要这个教程？

你正在开发钓鱼游戏，遇到了这样的问题：

```
❌ 鱼线调参太难了
   - Spring参数有20+个
   - 改一个参数，整体感受都变了
   - 凭感觉调了一周，还是不满意

❌ 手工调优效率低
   - 每次修改都要重新打包测试
   - 没有系统的调优方向
   - 经常调出一个问题，又引入另一个问题

❌ 参数稳定性差
   - 在办公室调得很好
   - 到了玩家手机上又出问题
   - 不同场景参数需要不同
```

**AI参数调优的承诺**：
- ✅ 自动搜索最优参数组合
- ✅ 在 1-2 天内找到比手工调参更好的参数
- ✅ 有科学的优化记录和可追溯性
- ✅ 可以根据不同场景快速生成参数

---

## 一、核心概念（5分钟速览）

### 1.1 什么是参数调优？

**定义**：找到参数的最优组合，使得某个目标函数（评分）最大化。

```
参数空间                    评分
  ↓                         ↑
[100, 0.5, 1.0, 50]   ──→  85.2 ✓
[120, 0.6, 1.2, 60]   ──→  87.5 ✓✓
[128, 0.62, 1.1, 58]  ──→  91.3 ✓✓✓ ← 最优
```

### 1.2 为什么传统方法失效？

**参数空间爆炸**：
- 20个参数，每个参数100种可能 = 100²⁰ 种组合
- 即使每秒测试1000个，也需要 10³⁸ 秒 = 无穷大 ⏱️

**人类的局限**：
- 凭直觉调试是高维度的盲目搜索
- 容易陷入局部最优
- 无法同时权衡多个目标

### 1.3 AI方案的优势

**贝叶斯优化**（推荐初学者使用）：
- 🎯 智能搜索：每次评估都学习参数空间
- 📊 样本高效：30-50次评估就能找到最优点
- 📈 自动权衡：在未知区域探索 vs 在已知区域利用
- 💰 成本低：只需单核Python脚本

**vs 其他方案**：
| 方案 | 初学难度 | 收敛速度 | 代码量 | 推荐度 |
|------|--------|--------|-------|-------|
| 贝叶斯优化 | ⭐ 简单 | ⭐⭐⭐ 快 | 50行 | ⭐⭐⭐⭐⭐ |
| 遗传算法 | ⭐⭐ 中等 | ⭐⭐ 较慢 | 100行 | ⭐⭐⭐⭐ |
| 强化学习 | ⭐⭐⭐ 复杂 | ⭐⭐⭐⭐ 很快 | 500行 | ⭐⭐⭐ |

---

## 二、完整案例：钓鱼游戏鱼线参数调优

### 2.1 案例背景

你的钓鱼游戏有这样的参数需要调优：

```csharp
// FishingRod.cs
public class FishingRodPhysics
{
    // 需要调优的参数（共5个）
    public float springConstant = 100f;        // 线的弹性系数
    public float frictionConstant = 0.5f;      // 线的阻尼
    public float impulseTolerance = 0.01f;     // 冲量阈值
    public float maxLineVelocity = 100f;       // 线的最大速度
    public float fishMass = 0.1f;              // 鱼的质量
}
```

**目标**：自动找到这5个参数的最优组合，使得"钓鱼手感"评分最高。

**手感评分的定义**（你需要定义）：
- 40% 稳定性：线不抖动 (加速度方差小)
- 30% 响应性：竿子输入立即反馈 (延迟小)
- 30% 平滑度：线条拉动自然流畅 (高频抖动少)

### 2.2 环境准备（15分钟）

**第1步：安装Python 3.9+**

```bash
# 检查Python版本
python --version

# 应该看到 Python 3.9.x 或更高
```

**第2步：安装必要的库**

```bash
# 创建虚拟环境（可选但推荐）
python -m venv venv

# Windows激活
venv\Scripts\activate
# Mac/Linux激活
source venv/bin/activate

# 安装库
pip install bayesian-optimization numpy scipy json
```

检查安装：
```bash
python -c "from bayes_opt import BayesianOptimization; print('✓ 安装成功')"
```

**第3步：准备项目结构**

```
F:\fishing-game-project\
├── FishingGame.exe           (你的游戏程序)
├── ai_tuning/
│   ├── optimizer.py          (优化脚本)
│   ├── config.json           (参数配置)
│   └── results.json          (优化结果)
└── physics_configs/
    └── default.json          (默认参数)
```

### 2.3 第一阶段：创建评估函数（最关键）

这是整个系统的核心。评估函数决定了"什么是好的参数"。

**FishingGame.cs** (C#端 - 在你的Unity项目中)

```csharp
// 添加自动评估模式
public class AutoEvaluationMode : MonoBehaviour
{
    private FishingRodPhysics rodPhysics;
    private List<float> stabilityData = new();
    private List<float> responseData = new();
    private List<float> smoothnessData = new();

    // 从JSON配置加载参数
    public void LoadParametersFromJson(string configPath)
    {
        string json = System.IO.File.ReadAllText(configPath);
        var config = JsonUtility.FromJson<FishingRodPhysics>(json);

        rodPhysics.springConstant = config.springConstant;
        rodPhysics.frictionConstant = config.frictionConstant;
        rodPhysics.impulseTolerance = config.impulseTolerance;
        rodPhysics.maxLineVelocity = config.maxLineVelocity;
        rodPhysics.fishMass = config.fishMass;
    }

    // 自动模拟钓鱼操作
    public void SimulateAutoFishing(float durationSeconds)
    {
        float elapsed = 0f;

        while (elapsed < durationSeconds)
        {
            // AI操作：周期性拉竿
            float rodInput = Mathf.Sin(elapsed * 2f) * 50f;  // 振幅50

            // 更新物理
            rodPhysics.Update(Time.deltaTime, rodInput);

            // 记录数据
            RecordMetrics();

            elapsed += Time.deltaTime;
        }
    }

    private void RecordMetrics()
    {
        // 记录线张力变化（用于计算稳定性）
        stabilityData.Add(rodPhysics.lineTension);

        // 记录延迟（用于计算响应性）
        responseData.Add(rodPhysics.responseLatency);

        // 记录加速度（用于计算平滑度）
        smoothnessData.Add(rodPhysics.lineAcceleration);
    }

    // 计算综合评分
    public float ComputeHandFeelScore()
    {
        float stability = ComputeStability();      // 0-100
        float responsiveness = ComputeResponsiveness();  // 0-100
        float smoothness = ComputeSmoothness();    // 0-100

        // 加权平均
        float score = stability * 0.4f +
                      responsiveness * 0.3f +
                      smoothness * 0.3f;

        return Mathf.Clamp(score, 0f, 100f);
    }

    private float ComputeStability()
    {
        // 张力方差越小 = 越稳定
        float mean = stabilityData.Average();
        float variance = stabilityData.Select(x => (x - mean) * (x - mean)).Average();

        // 将方差转换为0-100的分数
        // 方差为0时得100分，方差很大时得分很低
        float stabilityScore = 100f * Mathf.Exp(-variance / 100f);
        return stabilityScore;
    }

    private float ComputeResponsiveness()
    {
        // 平均延迟越小 = 越响应
        float avgLatency = responseData.Average();

        // 延迟0ms得100分，延迟100ms得0分
        float responsivenessScore = Mathf.Max(0, 100f - avgLatency);
        return responsivenessScore;
    }

    private float ComputeSmoothness()
    {
        // 高频抖动越少 = 越平滑
        // 计算加速度的变化率（二阶导数）
        float smoothnessScore = 100f;

        for (int i = 1; i < smoothnessData.Count; i++)
        {
            float jerk = Mathf.Abs(smoothnessData[i] - smoothnessData[i-1]);
            smoothnessScore -= jerk * 0.01f;  // 每单位jerk扣0.01分
        }

        return Mathf.Clamp(smoothnessScore, 0f, 100f);
    }

    // 保存评分结果
    public void SaveEvaluationResult(float score)
    {
        var result = new { hand_feel_score = score };
        string json = JsonUtility.ToJson(result);
        System.IO.File.WriteAllText("eval_result.json", json);
    }
}
```

**自动评估的三个关键指标**：

1️⃣ **稳定性** (Stability) - 线张力变化的平稳度
```
稳定的参数：线张力 [50, 51, 50, 52, 51] (方差小)  ✓
不稳定的参数：线张力 [20, 80, 10, 90, 15] (方差大) ✗
```

2️⃣ **响应性** (Responsiveness) - 竿子输入到线反应的延迟
```
好的参数：输入后 10ms 线张力就有反应  ✓
差的参数：输入后 100ms 才有反应 ✗
```

3️⃣ **平滑度** (Smoothness) - 线条拉动的流畅度
```
光滑的参数：线条平缓移动，无抖动 ✓
抖动的参数：线条高频颤抖 ✗
```

### 2.4 第二阶段：Python优化脚本

**ai_tuning/optimizer.py**

```python
import json
import subprocess
import numpy as np
from bayes_opt import BayesianOptimization

class FishingGameOptimizer:
    def __init__(self, game_exe_path):
        self.game_exe = game_exe_path
        self.history = []
        self.iteration = 0

    def evaluate_params(self, spring_constant, friction_constant,
                       impulse_tolerance, max_line_velocity, fish_mass):
        """
        评估参数组合的质量
        这是贝叶斯优化的目标函数
        """
        self.iteration += 1

        # 第1步：创建参数配置文件
        config = {
            'springConstant': spring_constant,
            'frictionConstant': friction_constant,
            'impulseTolerance': impulse_tolerance,
            'maxLineVelocity': max_line_velocity,
            'fishMass': fish_mass
        }

        with open('physics_configs/current.json', 'w') as f:
            json.dump(config, f)

        print(f"\n【迭代 {self.iteration}】")
        print(f"  测试参数：")
        print(f"    - springConstant: {spring_constant:.1f}")
        print(f"    - frictionConstant: {friction_constant:.2f}")
        print(f"    - impulseTolerance: {impulse_tolerance:.4f}")
        print(f"    - maxLineVelocity: {max_line_velocity:.1f}")
        print(f"    - fishMass: {fish_mass:.3f}")

        # 第2步：启动游戏进程，自动评估
        try:
            result = subprocess.run(
                [self.game_exe, '--mode', 'auto_eval', '--duration', '10'],
                timeout=30,  # 30秒超时
                capture_output=True
            )

            # 第3步：读取评估结果
            with open('eval_result.json', 'r') as f:
                eval_result = json.load(f)
                score = eval_result['hand_feel_score']

            print(f"  ➜ 评分：{score:.1f} / 100")

            # 记录历史
            self.history.append({
                'iteration': self.iteration,
                'params': config,
                'score': score
            })

            return score

        except subprocess.TimeoutExpired:
            print(f"  ⚠️ 评估超时，返回0分")
            return 0
        except Exception as e:
            print(f"  ❌ 错误：{e}")
            return 0

    def optimize(self):
        """运行贝叶斯优化"""

        # 定义参数范围
        param_bounds = {
            'spring_constant': (50, 200),           # 线弹性：50-200
            'friction_constant': (0.1, 1.0),        # 阻尼：0.1-1.0
            'impulse_tolerance': (0.001, 0.05),     # 冲量阈值：0.001-0.05
            'max_line_velocity': (50, 150),         # 最大速度：50-150
            'fish_mass': (0.05, 0.5)                # 鱼质量：0.05-0.5
        }

        # 创建贝叶斯优化器
        optimizer = BayesianOptimization(
            f=self.evaluate_params,
            pbounds=param_bounds,
            random_state=42,
            allow_duplicate_points=False
        )

        print("=" * 60)
        print("🎣 钓鱼游戏参数自动调优开始")
        print("=" * 60)
        print(f"参数空间：5个参数，每个参数范围见上")
        print(f"优化策略：贝叶斯优化（高斯过程）")
        print(f"样本数：初始5个 + 迭代25个 = 30个总样本")
        print()

        # 运行优化
        optimizer.maximize(
            init_points=5,      # 初始随机采样5次
            n_iter=25,          # 后续贝叶斯迭代25次
            acq='ucb'           # 采用上置信界策略
        )

        print("\n" + "=" * 60)
        print("🏆 优化完成！")
        print("=" * 60)

        # 获取最优参数
        best_params = optimizer.max['params']
        best_score = optimizer.max['target']

        print(f"\n最优评分：{best_score:.2f} / 100")
        print(f"\n最优参数组合：")
        print(f"  springConstant = {best_params['spring_constant']:.1f}")
        print(f"  frictionConstant = {best_params['friction_constant']:.3f}")
        print(f"  impulseTolerance = {best_params['impulse_tolerance']:.5f}")
        print(f"  maxLineVelocity = {best_params['max_line_velocity']:.1f}")
        print(f"  fishMass = {best_params['fish_mass']:.4f}")

        # 保存结果
        self.save_results(best_params, best_score)

        return best_params, best_score

    def save_results(self, best_params, best_score):
        """保存优化结果"""

        result_data = {
            'best_score': best_score,
            'best_params': best_params,
            'history': self.history,
            'total_iterations': self.iteration
        }

        with open('ai_tuning/results.json', 'w') as f:
            json.dump(result_data, f, indent=2)

        print(f"\n✓ 结果已保存到 ai_tuning/results.json")
        print(f"✓ 评估历史已记录（{self.iteration}个数据点）")

# 使用示例
if __name__ == '__main__':
    # 指定你的游戏程序路径
    optimizer = FishingGameOptimizer(
        game_exe_path='F:\\fishing-game-project\\FishingGame.exe'
    )

    # 开始优化
    best_params, best_score = optimizer.optimize()
```

### 2.5 完整的运行流程

**Step 1: 准备阶段**
```bash
# 进入项目目录
cd F:\fishing-game-project

# 激活Python环境
venv\Scripts\activate

# 确保配置文件存在
mkdir physics_configs
echo {} > physics_configs/default.json
```

**Step 2: 运行优化**
```bash
python ai_tuning/optimizer.py
```

**预期输出**：
```
============================================================
🎣 钓鱼游戏参数自动调优开始
============================================================
参数空间：5个参数，每个参数范围见上
优化策略：贝叶斯优化（高斯过程）
样本数：初始5个 + 迭代25个 = 30个总样本

【迭代 1】
  测试参数：
    - springConstant: 127.3
    - frictionConstant: 0.523
    - impulseTolerance: 0.0245
    - maxLineVelocity: 98.7
    - fishMass: 0.187
  ➜ 评分：62.5 / 100

【迭代 2】
  测试参数：
    - springConstant: 142.8
    - frictionConstant: 0.641
    - impulseTolerance: 0.0312
    - maxLineVelocity: 115.2
    - fishMass: 0.203
  ➜ 评分：75.3 / 100

... (继续迭代到第30次) ...

【迭代 30】
  测试参数：
    - springConstant: 135.2
    - frictionConstant: 0.598
    - impulseTolerance: 0.0268
    - maxLineVelocity: 107.5
    - fishMass: 0.195
  ➜ 评分：91.7 / 100

============================================================
🏆 优化完成！
============================================================

最优评分：91.7 / 100

最优参数组合：
  springConstant = 135.2
  frictionConstant = 0.598
  impulseTolerance = 0.0268
  maxLineVelocity = 107.5
  fishMass = 0.195

✓ 结果已保存到 ai_tuning/results.json
✓ 评估历史已记录（30个数据点）
```

### 2.6 结果分析

**ai_tuning/results.json**：
```json
{
  "best_score": 91.7,
  "best_params": {
    "spring_constant": 135.2,
    "friction_constant": 0.598,
    "impulse_tolerance": 0.0268,
    "max_line_velocity": 107.5,
    "fish_mass": 0.195
  },
  "history": [
    {
      "iteration": 1,
      "params": {...},
      "score": 62.5
    },
    ...
    {
      "iteration": 30,
      "params": {...},
      "score": 91.7
    }
  ],
  "total_iterations": 30
}
```

**关键对比**：
```
参数                    手工调优值    AI优化值     改进
─────────────────────────────────────────────────────
springConstant          100.0        135.2       +35.2%
frictionConstant        0.50         0.598       +19.6%
impulseTolerance        0.01         0.0268      +168%
maxLineVelocity         100.0        107.5       +7.5%
fishMass                0.10         0.195       +95%

评分                    78.5         91.7        +16.8%
```

---

## 三、常见问题解决

### Q1: 评估函数设计得不好怎么办？

**症状**：优化出来的参数得分很高，但玩家觉得很奇怪

**原因**：评估函数没有真实反映"手感"

**解决方案**：
```csharp
// ❌ 不好的评估函数（太简单）
float score = 100 - Mathf.Abs(lineTension - targetTension);

// ✅ 好的评估函数（多维度）
float stability = 1 - (tensionVariance / maxVariance);    // 40%
float responsiveness = 1 - (avgLatency / maxLatency);     // 30%
float smoothness = 1 - (acceleration.magnitude / maxAcc);  // 30%
float score = stability * 0.4f + responsiveness * 0.3f + smoothness * 0.3f;

// ✅✅ 更好的评估函数（包含约束）
// 检查是否满足约束条件
if (lineTension > maxTension) return 0;  // 线不能断
if (lineTension < minTension) return 0;  // 线必须有张力

// 多目标平衡
float score = ...
```

**建议**：
1. 先用简单的评估函数快速验证流程
2. 逐步添加更多维度
3. 记录玩家反馈，及时调整权重

### Q2: 优化速度太慢了怎么办？

**原因**：
- 评估函数执行时间过长
- 游戏启动时间过长
- 网络延迟

**解决方案**：
```python
# 方案1：并行评估（多进程）
from multiprocessing import Pool

def parallel_evaluate(param_list):
    with Pool(4) as pool:  # 4个进程并行
        results = pool.map(evaluate_params, param_list)
    return results

# 方案2：缓存评估结果
import hashlib

evaluated_params = {}

def evaluate_with_cache(**params):
    key = hashlib.md5(str(params).encode()).hexdigest()
    if key in evaluated_params:
        return evaluated_params[key]

    score = evaluate_params(**params)
    evaluated_params[key] = score
    return score

# 方案3：减少单次评估时间
# 改为5秒而不是10秒
subprocess.run([game_exe, '--duration', '5'])  # 时间减半
```

### Q3: 优化收敛到了局部最优怎么办？

**症状**：评分总是在80分徘徊，突破不了

**原因**：贝叶斯优化在一个小区域反复优化

**解决方案**：
```python
# 方案1：重启优化并调整参数范围
optimizer = BayesianOptimization(
    f=self.evaluate_params,
    pbounds=param_bounds,
    random_state=None  # 改为None，使用随机种子
)

# 方案2：使用遗传算法进行全局搜索
from genetic_algorithm import GeneticOptimizer
ga = GeneticOptimizer(
    population_size=50,
    generations=100,
    mutation_rate=0.2
)
best_params = ga.optimize(self.evaluate_params, param_bounds)

# 方案3：多起点搜索
best_results = []
for seed in range(5):  # 运行5次，不同随机种子
    optimizer = BayesianOptimization(
        f=self.evaluate_params,
        pbounds=param_bounds,
        random_state=seed
    )
    optimizer.maximize(init_points=3, n_iter=15)
    best_results.append(optimizer.max)

# 取最好的结果
best_overall = max(best_results, key=lambda x: x['target'])
```

### Q4: 不同玩家反馈参数应该怎么调？

**方案1：生成多个参数档位**
```python
# 根据不同的评估函数，生成多套参数
profiles = {
    'casual': {          # 休闲玩家（易操作）
        'weights': {'stability': 0.5, 'responsiveness': 0.3, 'smoothness': 0.2}
    },
    'hardcore': {        # 硬核玩家（高精度）
        'weights': {'stability': 0.2, 'responsiveness': 0.5, 'smoothness': 0.3}
    },
    'balanced': {        # 平衡（推荐）
        'weights': {'stability': 0.4, 'responsiveness': 0.3, 'smoothness': 0.3}
    }
}

# 为每个档位优化参数
for profile_name, profile_config in profiles.items():
    optimizer.evaluate_params = lambda **p: evaluate_with_weights(
        **p,
        weights=profile_config['weights']
    )
    best_params = optimizer.optimize()
    save_params(f"profile_{profile_name}.json", best_params)
```

---

## 四、进阶优化技巧

### 4.1 灵敏度分析：找出最关键的参数

不是所有参数都同样重要。

```python
def sensitivity_analysis(base_params, param_bounds):
    """
    分析每个参数对评分的影响
    """
    base_score = evaluate_params(**base_params)
    sensitivities = {}

    for param_name, (min_val, max_val) in param_bounds.items():
        # 在参数范围的25%, 50%, 75%处测试
        test_values = [
            min_val + (max_val - min_val) * 0.25,
            min_val + (max_val - max_val) * 0.50,
            min_val + (max_val - max_val) * 0.75,
        ]

        scores = []
        for val in test_values:
            test_params = base_params.copy()
            test_params[param_name] = val
            score = evaluate_params(**test_params)
            scores.append(score)

        # 参数影响度 = 分数变化范围
        sensitivity = max(scores) - min(scores)
        sensitivities[param_name] = sensitivity

    # 排序打印
    for param, sensitivity in sorted(sensitivities.items(),
                                     key=lambda x: x[1], reverse=True):
        print(f"{param}: 影响度 {sensitivity:.1f}")

    return sensitivities
```

**可能的结果**：
```
springConstant: 影响度 28.5  ← 最关键！改这个效果最大
frictionConstant: 影响度 15.2
maxLineVelocity: 影响度 8.3
impulseTolerance: 影响度 3.1
fishMass: 影响度 1.2    ← 改这个几乎没效果
```

**优化策略**：
- 优先优化高影响度参数
- 低影响度参数可以用默认值
- 这样可以大幅降低搜索空间维度

### 4.2 参数联动：发现参数之间的关系

某些参数必须一起调整才能发挥效果。

```python
def find_parameter_correlations(history_data):
    """
    分析历史记录中的参数相关性
    """
    import numpy as np

    params_list = []
    scores_list = []

    for record in history_data:
        params_list.append(list(record['params'].values()))
        scores_list.append(record['score'])

    params_array = np.array(params_list)
    scores_array = np.array(scores_list)

    # 计算每个参数与评分的相关性
    for i, param_name in enumerate(param_names):
        correlation = np.corrcoef(params_array[:, i], scores_array)[0, 1]
        print(f"{param_name}: 与评分的相关系数 {correlation:.3f}")
```

**高相关性** (0.7-1.0) = 这个参数很重要
**中等相关性** (0.3-0.7) = 这个参数有影响
**低相关性** (-0.3-0.3) = 这个参数可能不重要

### 4.3 增量式优化：基于旧参数继续优化

```python
def incremental_optimize(old_best_params, param_bounds):
    """
    基于已知的好参数，在周围区域精细搜索
    """

    # 缩小参数范围（在旧最优值周围±20%）
    refined_bounds = {}
    for param_name, (old_min, old_max) in param_bounds.items():
        old_val = old_best_params[param_name]
        range_size = old_max - old_min

        new_min = max(old_min, old_val - range_size * 0.2)
        new_max = min(old_max, old_val + range_size * 0.2)

        refined_bounds[param_name] = (new_min, new_max)

    # 用缩小的范围重新优化（更少迭代即可收敛）
    optimizer = BayesianOptimization(
        f=evaluate_params,
        pbounds=refined_bounds,
        random_state=42
    )

    optimizer.maximize(init_points=2, n_iter=10)  # 减少到12次

    return optimizer.max['params']
```

---

## 五、实战检查清单

使用此清单确保你的优化系统工作正常：

```
前期准备
☐ Python环境已安装，版本 ≥ 3.9
☐ 已安装 bayesian-optimization
☐ 游戏程序支持 --mode auto_eval 命令行参数
☐ 已定义3-5个关键参数要优化

评估函数
☐ ComputeHandFeelScore() 有返回值 0-100
☐ 三个评分指标都能计算（稳定性、响应性、平滑度）
☐ 评估时间 < 15秒（包括游戏启动）
☐ 评估函数结果可重复（同参数多次运行基本相同）

优化脚本
☐ evaluate_params() 能成功调用游戏程序
☐ 能正确读取 eval_result.json
☐ 能正确保存优化结果
☐ 参数范围合理（不要太宽）

验证
☐ 手工运行一次，验证流程完整
☐ 查看 results.json，评分在上升
☐ 最终的最优参数玩起来确实更好
☐ 在多台设备上验证参数稳定性
```

---

## 六、从这里开始

**建议的学习路径**：

### 第1天（2小时）
1. ✅ 安装Python和必要库 (15分钟)
2. ✅ 理解本教程的第2.1-2.3节 (30分钟)
3. ✅ 在游戏代码中集成 AutoEvaluationMode (45分钟)
4. ✅ 手工测试评估函数 (30分钟)

### 第2天（2小时）
1. ✅ 编写 optimizer.py (60分钟)
2. ✅ 运行第一次优化 (30分钟)
3. ✅ 分析结果，调整评估函数 (30分钟)

### 第3天（1小时）
1. ✅ 迭代优化（根据结果调整参数范围）
2. ✅ 真实玩家测试最优参数
3. ✅ 将最优参数合并到正式版本

**总耗时**：约5小时 → 获得比手工调参快10倍的效果

---

## 七、下一步进阶

当你掌握了贝叶斯优化后，可以探索：

1. **遗传算法** - 用于参数众多（20+）的情况
2. **强化学习** - 用于实时动态调整参数
3. **多目标优化** - 同时优化多个相互冲突的目标
4. **参数转移** - 将一个游戏的最优参数用于另一个游戏

---

**需要帮助？** 查看本项目的 `AI_Parameter_Optimization.html` 了解更多理论细节。

**准备好开始了吗？** 👉 从第2.2节的环境准备开始！
