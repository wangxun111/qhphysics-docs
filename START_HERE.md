# 🚀 开始修改文档之前必读

**⚠️ 重要：请务必阅读本文件！**

本项目有**严格的质量规则**，违反规则会导致网站崩溃或功能破坏。
下次打开命令行时，请先看这个文件！

---

## 📋 5 分钟快速规则

### ✅ 修改前检查

```bash
# 1. 进入项目目录
cd F:/new/fishinggame/QHPlugins/QHForUnity

# 2. 修改任何文件
vim some_file.html

# 3. 立即检查（必做！）
bash validate_docs.sh

# 4. 如果有错，修复后再检查
bash validate_docs.sh

# 5. 确认通过后才能提交
git add .
git commit -m "docs: Your message"
git push origin main
```

### ❌ 绝对不要做这些事

| ❌ 禁止 | 原因 | 后果 |
|--------|------|------|
| 创建文件后不更新链接 | 链接失效 | 网站 404 |
| 忘记添加 `display: none` | Modal 挡住内容 | 网站无法使用 |
| 改名文件不改链接 | 链接错乱 | 网页加载失败 |
| 修改 fetch 路径不检查 | Markdown 加载失败 | 页面内容空白 |
| 提交前不运行检查 | 阻止提交 | 浪费时间 |

---

## 🎯 核心规则（必记！）

### 规则 1: 每次修改后都要检查
```bash
bash validate_docs.sh  # 必须通过！
```

**为什么重要？** 之前因为没有这个检查，出现了：
- ✗ XunPhysics 完整架构无法打开
- ✗ AI 入门教程的 UI 挡在屏幕中间
- ✗ 文件名不匹配导致链接失效

### 规则 2: 所有文件名必须用 XunPhysics
```
✅ XunPhysics_Architecture.html
❌ QHPhysics_Architecture.html  (已过期)
```

### 规则 3: 所有 HTML modal 必须隐藏
```css
.note-editor {
    position: fixed;
    /* ... 其他样式 ... */
    display: none;  /* ← 必须有这一行 */
}
```

### 规则 4: 文件对应关系必须正确
```
XunPhysics_Architecture.html  → 加载 complete_doc.md
AI_Tuning_Getting_Started.html → 加载 AI_Tuning_Getting_Started.md
AI_Parameter_Optimization.html → 加载 AI_Parameter_Optimization.md
PhysX_Analysis.html            → 加载 PhysX_Analysis.md
```

---

## 🔍 检查清单（提交前必做）

```
提交前检查清单：

[ ] 运行了 bash validate_docs.sh？
[ ] 所有项目都通过（绿色 ✅）？
[ ] 是否有错误（红色 ❌）？如果有，修复了吗？
[ ] 新增的文件是否在 Documentation_Hub.html 中添加了链接？
[ ] 改名的文件是否更新了所有指向它的链接？
[ ] 新的 HTML 文件是否添加了 display: none 的 modal？
[ ] fetch() 的 markdown 文件是否存在？

以上都确认后，才能提交！
```

---

## 📚 详细文档位置

如果忘记了，查看这些文件：

| 文档 | 位置 | 用途 |
|------|------|------|
| **快速参考** | `QUICK_REFERENCE.md` | 日常快速查阅 |
| **完整说明** | `TESTING_GUIDE.md` | 详细学习 |
| **系统概览** | `README_TESTING.md` | 理解设计思路 |
| **本文件** | `START_HERE.md` | 第一次了解 |

---

## ⚡ 最常用的 3 个命令

```bash
# 命令 1: 检查（修改后必做）
bash validate_docs.sh

# 命令 2: 查看检查详细过程
bash -x validate_docs.sh

# 命令 3: 在 Hook 失败时强制提交（不推荐）
git commit --no-verify -m "message"
```

---

## 🎓 学习路径

### 第一次使用（5 分钟）
1. 阅读本文件的"5 分钟快速规则"
2. 理解"核心规则"部分
3. 记住"检查清单"
4. 开始修改文档

### 遇到问题（10 分钟）
1. 运行 `bash validate_docs.sh`
2. 查看错误信息
3. 对照 `QUICK_REFERENCE.md` 的"常见修复"
4. 修复后再次运行检查

### 深入理解（30 分钟）
1. 阅读 `README_TESTING.md` 理解系统设计
2. 阅读 `TESTING_GUIDE.md` 学习所有细节
3. 查看脚本源码 `validate_docs.sh`

---

## 🚨 如果不按规则做会怎样？

### 场景 1: 修改后直接提交（没检查）
```bash
$ git add .
$ git commit -m "docs: Update"
🔍 运行文档自测...
❌ [AI_Tuning_Getting_Started.html] .note-editor 缺少 display: none

❌ 自测失败！请修复上面的错误后重新提交。
# ← 提交被阻止！必须修复后才能提交
```

### 场景 2: 创建新 HTML 但不更新链接
```bash
$ bash validate_docs.sh
❌ [new_page.html] 链接指向不存在的文件: missing_data.md
# ← 检查失败，提前发现问题！
```

### 场景 3: 改名文件但不更新引用
```bash
$ bash validate_docs.sh
❌ [Documentation_Hub.html] 链接指向不存在的文件: old_name.html
# ← 立即发现，避免网站崩溃！
```

---

## 💾 规则永久保存位置

这些规则被保存在：

```
F:/new/fishinggame/QHPlugins/QHForUnity/
├── START_HERE.md           ← 本文件（第一次看）
├── QUICK_REFERENCE.md      ← 日常参考
├── TESTING_GUIDE.md        ← 详细说明
├── README_TESTING.md       ← 系统概览
├── validate_docs.sh        ← 自动检查脚本
└── .git/hooks/pre-commit   ← Git 钩子（自动运行）
```

**即使你关闭命令行重新打开，这些文件也永远在这里！**

---

## 🔗 重要链接

- **在线文档中心**: https://wangxun111.github.io/qhphysics-docs/Documentation_Hub.html
- **GitHub 仓库**: https://github.com/wangxun111/qhphysics-docs
- **项目目录**: `F:/new/fishinggame/QHPlugins/QHForUnity`

---

## ❓ 常见问题

**Q: 我忘记了规则，怎么办？**
A: 打开 `QUICK_REFERENCE.md`，2 分钟快速复习。

**Q: 为什么要这么多规则？**
A: 防止重复出现链接错误、Modal 挡住内容、文件名不匹配等问题。这些规则就是从之前的错误中学到的！

**Q: 如果检查失败，会损坏什么吗？**
A: 不会。检查失败时只是阻止提交，不会损坏任何东西。修复后重新提交即可。

**Q: 可以跳过检查吗？**
A: 可以，用 `git commit --no-verify`，但非常不推荐！这样容易出现之前的问题。

---

## 🎯 记住最重要的一条规则

```
修改文件 → 运行 bash validate_docs.sh → 检查通过 → 才能提交
```

**就这么简单！**

---

## 📞 需要帮助？

| 情况 | 查看文件 |
|------|--------|
| 忘记命令 | `QUICK_REFERENCE.md` |
| 遇到错误 | `TESTING_GUIDE.md` |
| 想理解设计 | `README_TESTING.md` |
| 第一次使用 | **本文件 (START_HERE.md)** |

---

**最后更新：** 2026年2月26日
**版本：** v1.0
**状态：** ✅ 完全可用

**下次修改文档时，记得先看这个文件！** 🚀
