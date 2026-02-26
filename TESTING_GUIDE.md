# 📋 文档自测机制说明

## 概述

为了避免重复出现链接错误、文件名不匹配、模态框显示问题等常见错误，我们建立了**自动自测机制**。

## 工作原理

### 1. 自动测试脚本 (`validate_docs.sh`)

在每次 `git commit` 前自动运行，检查以下问题：

#### ✅ 检查项目

| 检查项 | 说明 | 修复建议 |
|--------|------|---------|
| HTML 链接完整性 | 检查所有 `href` 是否指向存在的文件 | 补全缺失的文件或修正链接 |
| Modal 隐藏状态 | 确保 `.note-editor` 和 `.note-overlay` 有 `display: none` | 添加 `display: none` 到 CSS |
| 文件名一致性 | 检查是否有旧的 QHPhysics 命名（应使用 XunPhysics） | 改名文件 |
| Markdown 引用 | 检查 `fetch()` 中的 markdown 文件是否存在 | 确保对应的 `.md` 文件存在 |

### 2. Git Pre-commit Hook

位置：`.git/hooks/pre-commit`

**工作流程：**
```
git commit 命令
    ↓
运行 validate_docs.sh
    ↓
检查是否有错误
    ↓
有错误 → 阻止提交，显示错误
有警告 → 继续提交（但显示警告）
通过 → 正常提交
```

## 使用方法

### 方法 1: 手动运行（提交前检查）

```bash
bash validate_docs.sh
```

**输出示例：**
```
🔍 开始文档自测...

ℹ️  检查 HTML 文件中的链接...
✅ Documentation_Hub.html: 链接正常 AI_Tuning_Getting_Started.html
✅ XunPhysics_Architecture.html: 链接正常 Documentation_Hub.html
...

ℹ️  检查 modal/overlay 是否正确隐藏...
✅ AI_Parameter_Optimization.html: note-editor 正确隐藏
✅ AI_Tuning_Getting_Started.html: note-overlay 正确隐藏
...

========================================================
自测结果总结
========================================================

✅ 所有检查通过！文档质量良好 🎉
```

### 方法 2: 自动运行（提交时）

```bash
git add .
git commit -m "docs: Update something"
```

**如果检查失败：**
```
🔍 运行文档自测...

❌ [AI_Parameter_Optimization.html] 链接指向不存在的文件: missing.html

❌ 自测失败！请修复上面的错误后重新提交。
跳过检查: git commit --no-verify
```

**强制提交（跳过检查）：**
```bash
git commit --no-verify -m "docs: Emergency fix"
```

## 常见问题修复

### 问题 1: "链接指向不存在的文件"

**错误信息：**
```
[Documentation_Hub.html] 链接指向不存在的文件: missing.html
```

**解决方案：**
1. 创建缺失的文件，或
2. 修正 HTML 中的链接路径

### 问题 2: ".note-editor 缺少 display: none"

**错误信息：**
```
[AI_Parameter_Optimization.html] .note-editor 缺少 display: none
```

**解决方案：**
```css
.note-editor {
    position: fixed;
    /* ... 其他样式 ... */
    display: none;  /* ← 添加这一行 */
}
```

### 问题 3: "fetch 指向不存在的文件"

**错误信息：**
```
[XunPhysics_Architecture.html] fetch 指向不存在的文件: architecture.md
```

**解决方案：**
1. 检查 HTML 中的 fetch 路径
2. 确保对应的 markdown 文件存在

### 问题 4: "发现旧命名文件"

**警告信息：**
```
⚠️ 发现旧命名文件: QHPhysics_Architecture.html (应该使用 XunPhysics)
```

**解决方案：**
```bash
mv QHPhysics_Architecture.html XunPhysics_Architecture.html
```

## 最佳实践

### ✅ 推荐流程

```bash
# 1. 做出修改
vim some_file.html

# 2. 在提交前手动测试（可选但推荐）
bash validate_docs.sh

# 3. 如果有错误，修复后再测试
bash validate_docs.sh

# 4. 修复完成，正常提交
git add .
git commit -m "docs: Update XYZ feature"
```

### ✅ 修改清单

在修改任何文档文件前，检查以下事项：

- [ ] 是否创建了新的 HTML 文件？
  - [ ] 是否在 Documentation_Hub.html 中添加了链接？
  - [ ] 链接路径是否正确？

- [ ] 是否修改了文件名？
  - [ ] 所有指向该文件的链接都已更新？
  - [ ] 是否使用了新的命名规范（XunPhysics）？

- [ ] 是否在 HTML 中添加了 note-editor 或 note-overlay？
  - [ ] 是否添加了 `display: none` 样式？

- [ ] 是否添加了新的 markdown 文件引用？
  - [ ] 对应的 `.md` 文件是否存在？
  - [ ] fetch 路径是否正确？

## 扩展功能

未来可以添加的检查项：

- [ ] HTML 语法验证
- [ ] CSS 验证
- [ ] 图片引用检查
- [ ] 外部链接有效性检查（需要网络）
- [ ] 内容重复检查
- [ ] Markdown 格式验证

## 脚本文件位置

```
F:/new/fishinggame/QHPlugins/QHForUnity/
├── validate_docs.sh          ← 主测试脚本
├── validate_docs.py          ← Python 版本（备用）
├── .git/hooks/pre-commit     ← Git 钩子（自动触发）
└── TESTING_GUIDE.md          ← 本文档
```

## 调试技巧

### 查看脚本的详细执行过程

```bash
bash -x validate_docs.sh
```

### 检查特定文件

```bash
# 只检查某个 HTML 文件
grep -A 5 "\.note-editor {" AI_Parameter_Optimization.html | grep "display"

# 检查所有链接
grep -o 'href="[^"]*"' Documentation_Hub.html
```

### 临时禁用 Git Hook

```bash
# 临时禁用钩子
chmod -x .git/hooks/pre-commit

# 重新启用钩子
chmod +x .git/hooks/pre-commit
```

## 快速参考

```bash
# 运行测试
bash validate_docs.sh

# 跳过 git hook 提交
git commit --no-verify -m "message"

# 强制检查 modal display
grep -B 5 "display: none" AI_Tuning_Getting_Started.html | head -20
```

---

**最后更新：** 2026年2月26日
**版本：** v1.0
**维护者：** Documentation Team
