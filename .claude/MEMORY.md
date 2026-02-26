# XunPhysics 文档项目 - 开发备忘录

## 项目架构

### 核心文档
- **XunPhysics_Architecture.html** - 完整架构文档（加载 complete_doc.md）
- **AI_Tuning_Getting_Started.html** - AI 入门教程（加载 AI_Tuning_Getting_Started.md）
- **AI_Parameter_Optimization.html** - AI 深度分析（加载 AI_Parameter_Optimization.md）
- **PhysX_Analysis.html** - PhysX 对标分析（加载 PhysX_Analysis.md）
- **Documentation_Hub.html** - 中央文档导航中心

### 数据源文件
- complete_doc.md - XunPhysics 完整架构（1007 行）
- AI_Tuning_Getting_Started.md - 入门教程（800+ 行）
- AI_Parameter_Optimization.md - 深度分析（923 行）
- PhysX_Analysis.md - 对标分析（540 行）

### 自测机制
- **validate_docs.sh** - 自动化文档检查脚本（bash）
- **validate_docs.py** - Python 版本（备用）
- **.git/hooks/pre-commit** - Git 提交前钩子（自动触发）
- **TESTING_GUIDE.md** - 自测使用说明文档

## 常见问题排查

### 问题1: HTML 链接指向不存在的文件
**原因：** 文件改名但未更新链接（如 QHPhysics → XunPhysics）
**检查：** `bash validate_docs.sh`
**修复：** 1) 改名文件 2) 更新所有指向它的链接

### 问题2: Modal UI 挡在屏幕中间
**原因：** CSS 中 `.note-editor` 或 `.note-overlay` 缺少 `display: none`
**检查：** `bash validate_docs.sh`
**修复：** 在 CSS 中添加 `display: none` 样式

### 问题3: fetch 加载失败
**原因：** HTML 中的 fetch() 路径指向不存在的 markdown
**检查：** 浏览器控制台查看 404 错误
**修复：** 确保 markdown 文件存在并路径正确

## 部署流程

1. 修改文档
2. 运行 `bash validate_docs.sh` 检查
3. 修复所有错误
4. `git add . && git commit -m "..."`
5. Git hook 自动运行检查
6. `git push origin main`（1-2 分钟后网站自动更新）

## 重要文件对应关系

| HTML 文件 | 加载的 Markdown | 存储位置 |
|----------|----------------|--------|
| XunPhysics_Architecture.html | complete_doc.md | F:/.../ |
| AI_Tuning_Getting_Started.html | AI_Tuning_Getting_Started.md | F:/.../ |
| AI_Parameter_Optimization.html | AI_Parameter_Optimization.md | F:/.../ |
| PhysX_Analysis.html | PhysX_Analysis.md | F:/.../ |

## GitHub Pages 部署信息

- **仓库地址：** https://github.com/wangxun111/qhphysics-docs
- **网站地址：** https://wangxun111.github.io/qhphysics-docs/Documentation_Hub.html
- **用户名：** wangxun111
- **部署方式：** GitHub Pages（从 main 分支的根目录）
