# 🚀 快速参考 - 修改文档时的操作流程

## ⚡ 最简单的流程（30秒）

```bash
# 1. 修改文件（使用编辑器或 vim）
# 2. 检查（1秒）
bash validate_docs.sh

# 3. 如果有错，修复后再检查

# 4. 提交并推送（自动进行自测）
git add .
git commit -m "docs: Update something"
git push origin main

# 完成！网站在 1-2 分钟后自动更新
```

## 📋 修改清单

**在提交前，检查以下事项：**

- [ ] HTML 文件内的所有链接都有效？
- [ ] `.note-editor` 和 `.note-overlay` 有 `display: none`？
- [ ] 所有文件使用 `XunPhysics` 命名（不是 QHPhysics）？
- [ ] HTML 中的 `fetch()` 指向的 markdown 文件存在？

**可以用脚本一键检查：**
```bash
bash validate_docs.sh
```

## 🔧 常见修复

### 链接错误
```
❌ [Documentation_Hub.html] 链接指向不存在的文件: missing.html

✅ 修复：创建文件或修正链接
```

### Modal 被挡
```
❌ [AI_Tuning_Getting_Started.html] .note-editor 缺少 display: none

✅ 修复：在 CSS 中添加 display: none
```

### 文件名不匹配
```
❌ 发现旧命名文件: QHPhysics_Architecture.html

✅ 修复：mv QHPhysics_Architecture.html XunPhysics_Architecture.html
```

### Markdown 加载失败
```
❌ [XunPhysics_Architecture.html] fetch 指向不存在的文件: missing.md

✅ 修复：确保 markdown 文件存在
```

## 🎯 核心概念

| 文件 | 作用 | 何时运行 |
|------|------|--------|
| validate_docs.sh | 检查文档问题 | 手动运行或自动（提交前） |
| .git/hooks/pre-commit | Git 钩子 | 每次 commit 前自动运行 |
| TESTING_GUIDE.md | 详细说明 | 遇到问题时查阅 |

## 📚 文档对应关系

```
XunPhysics_Architecture.html  ──加载──> complete_doc.md
AI_Tuning_Getting_Started.html ──加载──> AI_Tuning_Getting_Started.md
AI_Parameter_Optimization.html ──加载──> AI_Parameter_Optimization.md
PhysX_Analysis.html           ──加载──> PhysX_Analysis.md
```

## ⚡ 速查表

```bash
# 运行测试
bash validate_docs.sh

# 显示测试详细过程
bash -x validate_docs.sh

# 强制提交（跳过检查，不推荐）
git commit --no-verify -m "message"

# 查看特定文件中的 display:none
grep "display: none" AI_Tuning_Getting_Started.html

# 重新启用 Git Hook（如果禁用过）
chmod +x .git/hooks/pre-commit
```

## 🎉 成功提交示例

```
$ bash validate_docs.sh

✅ AI_Parameter_Optimization.html: 链接正常 Documentation_Hub.html
✅ XunPhysics_Architecture.html: note-editor 正确隐藏
✅ 文件名命名规范（使用 XunPhysics）
✅ XunPhysics_Architecture.html: 正确加载 complete_doc.md

✅ 所有检查通过！文档质量良好 🎉

$ git add .
$ git commit -m "docs: Fix typo in Spring chapter"

🔍 运行文档自测...
✅ 所有检查通过，继续提交...
[main abc1234] docs: Fix typo in Spring chapter
 1 file changed, 2 insertions(+)

$ git push origin main
To https://github.com/wangxun111/qhphysics-docs.git
   6e841a5..abc1234  main -> main

✅ 完成！网站 1-2 分钟后自动更新
```

---

**更多详情：** 查看 `TESTING_GUIDE.md`
