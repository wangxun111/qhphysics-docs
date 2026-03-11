# 🐛 GitHub Pages 部署问题复盘与分析报告

## 1. 核心差异：本地 vs 远端

| 特性 | 本地环境 (Windows + Python Server) | 远端环境 (GitHub Pages + Linux) | 导致的问题 |
| :--- | :--- | :--- | :--- |
| **文件系统** | **大小写不敏感** (Case-Insensitive) | **大小写敏感** (Case-Sensitive) | 本地能打开 `Scripts/xxx.ps1`，远端如果写成 `scripts/` 则报 404。 |
| **路径解析** | 相对路径通常由文件系统处理 | URL 路径解析，严格遵循 Web 标准 | 链接中多余的斜杠或空格在远端可能导致路径错误。 |
| **默认引擎** | 无 (静态文件直接服务) | **Jekyll** (默认静态生成器) | Jekyll 会自动忽略以 `_` 开头的文件（如 `_sidebar.md`）和文件夹。 |
| **网络协议** | `http://localhost` | `https://wangxun111.github.io` | 混用 HTTP/HTTPS 资源会导致“混合内容”报错；CDN 链接如果写 `//cdn...` 在本地 `file://` 下会失败。 |
| **更新机制** | 修改即时生效 | **构建延迟** (Build Latency) | 推送后需要 1-5 分钟构建，且浏览器缓存极重 (Cache)，导致看不到最新修改。 |

## 2. 历次故障原因深度汇总

### A. "网页一片空白" (The White Screen)
*   **原因**: 使用 `file:///` 协议直接打开 `index.html`。
*   **机制**: 浏览器的 CORS 策略（跨域资源共享）禁止 JavaScript 读取同目录下的 `.md` 或 `.json` 文件。
*   **解决**: 必须使用 HTTP 服务器（如 `PreviewSite.bat` 启动的 Python Server）。

### B. "侧边栏丢失" / "菜单无法加载"
*   **原因**: GitHub Pages 默认启用 **Jekyll** 处理。
*   **机制**: Jekyll 视下划线 `_` 为特殊保留前缀（如 `_posts`），因此它**不会发布** `_sidebar.md`。
*   **解决**: 必须在仓库根目录添加名为 `.nojekyll` 的空文件，禁用 Jekyll 过滤。

### C. "找不到 HTML 报表文件" (404 Not Found)
*   **原因**: Docsify 的路由劫持。
*   **机制**: Docsify 默认接管所有 URL。当你访问 `/html/report.html` 时，Docsify 可能会尝试把它当做 Markdown 路由去解析，而不是直接加载静态文件。
*   **解决**: 在侧边栏链接中添加 `':ignore'` 后缀（如 `[Report](path.html ':ignore')`），告诉 Docsify "这是外部文件，不要处理，直接跳转"。

### D. "同步失败" / "Connection Reset"
*   **原因**: 网络不稳定与 Git 历史冲突。
*   **解决**: 
    1. 增加重试机制 (Retry Logic)。
    2. 使用 `git push --force` 解决因反复回滚导致的历史分叉。

## 3. 标准化规范 (Standard Operating Procedure)

为了杜绝未来的环境差异问题，请严格遵守以下规范：

1.  **文件名规范**:
    *   **严禁**使用中文文件名（尽量避免，兼容性差）。
    *   **严禁**文件名中带空格（URL编码问题）。
    *   统一使用 **PascalCase** (如 `FishingRodSimulation`) 或 **snake_case**。

2.  **必须存在的文件**:
    *   `/.nojekyll`: 保证 `_sidebar.md` 能被读取。
    *   `/README.md`: Docsify 的默认首页入口。
    *   `/index.html`: 网站入口。

3.  **调试流程**:
    *   不要相信 `file://` 打开的效果。
    *   **必须**使用 `PreviewSite.bat` 在本地 `http://localhost:8000` 进行验证。
    *   本地验证通过后 --> 推送 --> 等待 3 分钟 --> **强制刷新** (Ctrl+F5) 查看远端。

---
*此文档由 AI 自动生成，归档于 KnowledgeBase*
