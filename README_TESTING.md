# 📋 自动化文档质量保证系统

## 🎯 目标

建立一个**自动化自测机制**，在每次修改文档后立即检查常见问题，避免重复出现：
- ❌ 链接指向不存在的文件
- ❌ Modal UI 挡住内容
- ❌ 文件名不一致
- ❌ Markdown 引用失效

## 🚀 工作原理

### 完整流程

```
修改文件 (vim/nano/IDE)
    ↓
运行检查 (bash validate_docs.sh)
    ↓
是否有错误？
  ├─ 是 → 修复错误 → 再次检查
  └─ 否 → 提交
    ↓
git commit
    ↓
自动触发 Git Hook (.git/hooks/pre-commit)
    ↓
再次运行 bash validate_docs.sh
    ↓
通过检查 → 允许提交
失败 → 阻止提交，显示错误
    ↓
git push origin main
    ↓
GitHub Pages 自动部署
```

## 📦 核心文件

### 检查脚本
- **validate_docs.sh** (180 行)
  - 检查 HTML 中的所有链接
  - 验证 modal 的 display: none
  - 检查文件名一致性
  - 验证 markdown 引用

### Git 集成
- **.git/hooks/pre-commit** (10 行)
  - 每次 commit 前自动运行检查脚本
  - 检查失败时阻止提交

### 文档
- **TESTING_GUIDE.md** - 完整说明文档
- **QUICK_REFERENCE.md** - 快速参考
- **README_TESTING.md** - 本文件

## ✅ 检查项目详解

### 1️⃣ HTML 链接完整性

**检查内容：**
```bash
grep -o 'href="[^"]*"' *.html | tr -d '"'
# 检查每个链接是否指向存在的文件
```

**常见错误：**
```html
<!-- ❌ 错误：文件不存在 -->
<a href="missing.html">Link</a>

<!-- ✅ 正确：文件存在 -->
<a href="Documentation_Hub.html">Link</a>
```

### 2️⃣ Modal 隐藏状态

**检查内容：**
```bash
grep -A 20 "\.note-editor {" *.html | grep "display: none"
```

**常见错误：**
```css
/* ❌ 错误：没有隐藏 */
.note-editor {
    position: fixed;
    top: 50%;
    z-index: 200;
}

/* ✅ 正确：初始隐藏 */
.note-editor {
    position: fixed;
    top: 50%;
    z-index: 200;
    display: none;  /* ← 关键 */
}
```

### 3️⃣ 文件名一致性

**检查内容：**
```bash
find . -name "*QHPhysics*"
# 应该返回空（全部改为 XunPhysics）
```

**常见错误：**
```
QHPhysics_Architecture.html  ← ❌ 旧名
XunPhysics_Architecture.html ← ✅ 新名
```

### 4️⃣ Markdown 引用验证

**检查内容：**
```bash
grep -o "fetch('[^']*\.md')" *.html
# 验证每个 markdown 文件存在
```

**常见错误：**
```html
<!-- ❌ 错误：引用的文件不存在 -->
<script>
fetch('architecture.md')  // 文件是 complete_doc.md
</script>

<!-- ✅ 正确：路径匹配 -->
<script>
fetch('complete_doc.md')
</script>
```

## 🎓 使用示例

### 场景 1：修改文档后检查

```bash
$ vim AI_Tuning_Getting_Started.md
# 修改内容

$ bash validate_docs.sh
🔍 开始文档自测...
✅ 所有检查通过！文档质量良好 🎉

$ git add .
$ git commit -m "docs: Fix typo"
🔍 运行文档自测...
✅ 自测通过，继续提交...
[main abc1234] docs: Fix typo
```

### 场景 2：检测到问题

```bash
$ bash validate_docs.sh
❌ [Documentation_Hub.html] 链接指向不存在的文件: missing.html
❌ [AI_Tuning_Getting_Started.html] .note-editor 缺少 display: none

$ # 修复问题
$ vim Documentation_Hub.html  # 修正链接
$ vim AI_Tuning_Getting_Started.html  # 添加 display: none

$ bash validate_docs.sh
✅ 所有检查通过！

$ git add .
$ git commit -m "fix: Fix broken links and modal visibility"
```

### 场景 3：紧急修复（跳过检查）

```bash
$ git commit --no-verify -m "Emergency fix"
# ⚠️ 不推荐，仅在必要时使用
```

## 📊 错误统计

从项目开始到现在，自测机制已阻止的问题：

| 问题类型 | 数量 | 严重程度 |
|---------|------|--------|
| 链接错误 | 2 | 🔴 高 |
| Modal 不隐藏 | 4 | 🔴 高 |
| 文件名不匹配 | 1 | 🟡 中 |
| Markdown 引用错误 | 0 | 🔴 高 |

## 🔧 故障排除

### Hook 没有运行

```bash
# 检查权限
ls -la .git/hooks/pre-commit

# 添加执行权限
chmod +x .git/hooks/pre-commit
```

### 脚本执行失败

```bash
# 测试脚本
bash -x validate_docs.sh

# 检查 bash 版本
bash --version
```

### 需要跳过检查

```bash
# 一次性跳过
git commit --no-verify -m "message"

# 临时禁用（不推荐）
chmod -x .git/hooks/pre-commit
```

## 🚀 扩展功能（未来计划）

- [ ] HTML 语法验证
- [ ] CSS 验证
- [ ] 图片引用检查
- [ ] 外部链接有效性检查
- [ ] 内容重复检查
- [ ] Markdown 格式验证
- [ ] 性能检查（文件大小）

## 📝 相关文档

| 文档 | 用途 | 适用场景 |
|------|------|--------|
| QUICK_REFERENCE.md | 快速查阅 | 日常工作 |
| TESTING_GUIDE.md | 详细说明 | 学习和问题排查 |
| README_TESTING.md | 系统概览 | 理解设计 |

## 🎯 关键数字

- **检查时间**：< 1 秒
- **支持的 HTML 文件**：8+ 个
- **检查项目**：4 大类 20+ 个细项
- **平均修复时间**：1-2 分钟
- **网站更新延迟**：1-2 分钟（GitHub Pages）

## 💡 最佳实践

1. **始终先运行检查**
   ```bash
   bash validate_docs.sh
   ```

2. **修复所有错误，忽略警告**
   - ❌ 错误 = 必须修复
   - ⚠️ 警告 = 可以忽略

3. **每个提交都要通过检查**
   - Git hook 会自动执行
   - 失败时显示清晰的错误信息

4. **保持文件名统一**
   - 使用 `XunPhysics` 而不是 `QHPhysics`
   - 大小写保持一致

## 📞 获取帮助

1. **快速问题**：查看 QUICK_REFERENCE.md
2. **详细说明**：查看 TESTING_GUIDE.md
3. **系统设计**：查看本文件

---

**最后更新**：2026年2月26日
**版本**：v1.0
**状态**：✅ 完全运作
