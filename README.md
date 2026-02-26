# 📖 XunPhysics 文档项目

> 一个自动化质量保证的物理引擎文档网站

**🚀 在线访问：** https://wangxun111.github.io/qhphysics-docs/

---

## ⚠️ 首先请阅读

**如果这是你第一次接触本项目，请立即打开：**

📄 **[START_HERE.md](START_HERE.md)** ← 必读！5 分钟了解所有规则

---

## 🎯 核心工作流（30秒）

```bash
# 1. 修改文档
vim some_file.html

# 2. 检查（必须通过！）
bash validate_docs.sh

# 3. 如果有错，修复后再检查
bash validate_docs.sh

# 4. 提交并推送
git add .
git commit -m "docs: Your message"
git push origin main
```

---

## 🔑 5 条必记规则

| # | 规则 | 后果 |
|----|------|------|
| 1️⃣ | 修改后必须运行 `bash validate_docs.sh` | 链接错误、网页崩溃 |
| 2️⃣ | 所有文件名用 `XunPhysics` 不用 `QHPhysics` | 链接失效 |
| 3️⃣ | HTML 中必须有 `.note-editor { display: none; }` | Modal 挡住内容 |
| 4️⃣ | fetch() 的 markdown 文件必须存在 | 页面空白 |
| 5️⃣ | 新建文件要在 Documentation_Hub.html 添加链接 | 文档找不到 |

---

## 📚 快速链接

### 📋 文档
- **[START_HERE.md](START_HERE.md)** - 第一次了解（必读！）
- **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** - 日常快速参考
- **[TESTING_GUIDE.md](TESTING_GUIDE.md)** - 详细使用说明
- **[README_TESTING.md](README_TESTING.md)** - 系统设计概览

### 🛠️ 脚本
- **[validate_docs.sh](validate_docs.sh)** - 自动检查脚本
- **[.git/hooks/pre-commit](.git/hooks/pre-commit)** - Git 自动钩子

### 🌐 在线
- **[文档中心](https://wangxun111.github.io/qhphysics-docs/Documentation_Hub.html)** - 完整网站
- **[GitHub 仓库](https://github.com/wangxun111/qhphysics-docs)** - 源代码

---

## ✅ 检查项目

本项目的自动化检查会验证：

```
✓ HTML 中所有链接是否指向存在的文件
✓ Modal 是否正确隐藏（display: none）
✓ 文件名是否使用正确的命名规范
✓ fetch() 是否指向存在的 markdown 文件
```

全部自动化，< 1 秒完成！

---

## 🚀 常用命令速查

```bash
# 检查文档（修改后必做）
bash validate_docs.sh

# 查看检查的详细过程
bash -x validate_docs.sh

# 提交代码
git add .
git commit -m "docs: Your message"

# 推送到 GitHub（自动部署）
git push origin main

# 紧急情况下跳过检查（不推荐）
git commit --no-verify -m "Emergency fix"
```

---

## 🎓 学习资源

### 初次使用（5 分钟）
👉 打开 **[START_HERE.md](START_HERE.md)**

### 日常查阅（2 分钟）
👉 打开 **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)**

### 深入学习（30 分钟）
👉 打开 **[TESTING_GUIDE.md](TESTING_GUIDE.md)** 和 **[README_TESTING.md](README_TESTING.md)**

---

## 📁 项目结构

```
F:/new/fishinggame/QHPlugins/QHForUnity/
│
├── 📚 文档（在线访问）
├── XunPhysics_Architecture.html         ← 物理引擎架构
├── AI_Tuning_Getting_Started.html       ← AI 入门教程
├── AI_Parameter_Optimization.html       ← AI 深度分析
├── PhysX_Analysis.html                  ← PhysX 对标
├── Documentation_Hub.html               ← 文档中心（主页）
│
├── 📖 数据文件
├── complete_doc.md                      ← XunPhysics 完整文档
├── AI_Tuning_Getting_Started.md
├── AI_Parameter_Optimization.md
├── PhysX_Analysis.md
│
├── 🔍 检查系统
├── validate_docs.sh                     ← 主检查脚本
├── validate_docs.py                     ← Python 版本（备用）
├── .git/hooks/pre-commit                ← Git 自动钩子
│
└── 📖 规则和文档
    ├── README.md                        ← 本文件
    ├── START_HERE.md                    ← 新手必读
    ├── QUICK_REFERENCE.md               ← 快速参考
    ├── TESTING_GUIDE.md                 ← 详细说明
    └── README_TESTING.md                ← 系统概览
```

---

## 🛡️ 质量保证

本项目使用自动化检查系统确保质量：

- ✅ 每次 commit 前自动运行检查
- ✅ 检查失败时阻止提交
- ✅ 清晰的错误提示和修复建议
- ✅ 无需手动检查，完全自动化

**结果：** 零错误的网站，永远稳定！

---

## 💡 为什么需要这些规则？

之前没有这个系统时出现过的问题：

| 问题 | 原因 | 现在状态 |
|------|------|--------|
| XunPhysics 架构无法打开 | 文件名不匹配 | ✅ 自动检查 |
| Modal UI 挡住内容 | 缺少 display: none | ✅ 自动检查 |
| 链接 404 错误 | 文件不存在或命名错误 | ✅ 自动检查 |
| Markdown 加载失败 | 路径不正确 | ✅ 自动检查 |

**这些规则就是从错误中学到的！**

---

## 📝 修改前检查清单

```
准备修改代码？先检查这个清单：

[ ] 已打开 START_HERE.md 了吗？
[ ] 了解 5 条核心规则吗？
[ ] 知道修改后要运行 bash validate_docs.sh 吗？
[ ] 准备好了吗？开始修改！

修改完成后：

[ ] 运行 bash validate_docs.sh
[ ] 所有检查都通过了吗？（绿色 ✅）
[ ] 如果有错误，修复后再检查
[ ] 确认无误后，才能提交
```

---

## 🌐 在线访问

- **主页**：https://wangxun111.github.io/qhphysics-docs/
- **文档中心**：https://wangxun111.github.io/qhphysics-docs/Documentation_Hub.html
- **GitHub**：https://github.com/wangxun111/qhphysics-docs

---

## 📞 需要帮助？

| 问题 | 解决方案 |
|------|--------|
| 不知道从哪开始 | 打开 [START_HERE.md](START_HERE.md) |
| 忘记命令了 | 打开 [QUICK_REFERENCE.md](QUICK_REFERENCE.md) |
| 遇到错误信息 | 打开 [TESTING_GUIDE.md](TESTING_GUIDE.md) 的"常见问题" |
| 想理解系统 | 打开 [README_TESTING.md](README_TESTING.md) |

---

## 🎉 开始使用

**新手？从这里开始：**

```bash
# 1. 首先，用 15 秒打开并快速浏览 START_HERE.md
cat START_HERE.md | head -50

# 2. 理解核心规则后，修改你的文件

# 3. 每次修改后，运行检查
bash validate_docs.sh

# 4. 通过检查后，正常提交
git add .
git commit -m "docs: Your change"
git push origin main
```

---

**版本：** v1.0
**最后更新：** 2026年2月26日
**状态：** ✅ 完全运作

**记住：修改前阅读 [START_HERE.md](START_HERE.md)！** 🚀
