# 📖 仓库文件是怎样变成网页的？

> 本文解释 `wangxun111.github.io/qhphysics-docs` 这个网站的完整工作原理。

---

## 一、整体流程（一图概览）

```
你在本地修改文件
       ↓
git push → 推送到 GitHub 仓库（main 分支）
       ↓
GitHub Pages 自动将仓库文件发布到互联网
       ↓
用户访问 https://wangxun111.github.io/qhphysics-docs/
       ↓
浏览器下载 .html 文件并渲染
       ↓
页面内的 JavaScript 用 fetch() 下载 .md 文件
       ↓
marked.js 把 Markdown 文字转换成 HTML，注入页面
```

---

## 二、两类文件，两种处理方式

### 1. `.html` 文件 → 直接就是网页

仓库里的 `.html` 文件（比如 `Documentation_Hub.html`、`AI_Tuning_Getting_Started.html`）已经是完整的网页，不需要任何转换。  
GitHub Pages 把它们原封不动地发布到网上，浏览器打开就能看到带样式的页面。

### 2. `.md` 文件 → 在浏览器里动态转换成 HTML

仓库里的 `.md` 文件（比如 `AI_Tuning_Getting_Started.md`、`complete_doc.md`）**不是**直接被当成网页显示的。  
它们的内容由对应的 `.html` 文件在浏览器运行时加载和渲染，步骤如下：

```html
<!-- 第 1 步：HTML 页面加载 marked.js 库（CDN） -->
<script src="https://cdn.jsdelivr.net/npm/marked/marked.min.js"></script>

<!-- 第 2 步：JavaScript 用 fetch() 下载 .md 文件 -->
<script>
fetch('AI_Tuning_Getting_Started.md')        // 下载 Markdown 文件
  .then(response => response.text())          // 读取文本内容
  .then(markdown => {
    marked.setOptions({ breaks: true, gfm: true });
    // 第 3 步：marked.js 把 Markdown 转成 HTML，注入页面
    document.getElementById('content').innerHTML = marked.parse(markdown);
  });
</script>
```

**关键点**：转换发生在**用户的浏览器里**，不是在服务器上。GitHub Pages 只负责把文件传给浏览器，浏览器自己完成渲染。

---

## 三、GitHub Pages 是什么？

GitHub Pages 是 GitHub 提供的**免费静态网站托管服务**。

- 把仓库里的文件（HTML、CSS、JS、图片……）直接发布到公开网址
- **不支持**服务端语言（PHP、Python、Node.js 等），只能托管静态文件
- 网址格式：`https://<用户名>.github.io/<仓库名>/`

本仓库的网址：`https://wangxun111.github.io/qhphysics-docs/`

---

## 四、部署是怎么触发的？

### 旧方式（自动，已改为手动）

之前，每次 `git push` 到 `main` 分支，GitHub 都会自动运行内置的 **`pages build and deployment`** 工作流来发布网站，并发送邮件通知。

### 现在的方式（手动触发，无多余邮件）

现在使用仓库自定义的 **`.github/workflows/deploy-pages.yml`** 工作流，触发方式改为手动（`workflow_dispatch`），步骤如下：

```
git push  →  代码保存到 GitHub（不触发部署，不发邮件）

需要更新网站时：
  GitHub 仓库页面 → Actions → Deploy Pages → Run workflow → 手动触发
```

> 需要生效，还须在仓库 **Settings → Pages → Source** 中选择 **"GitHub Actions"**
> 而非 "Deploy from a branch"。详见 `.github/workflows/deploy-pages.yml` 顶部说明。

---

## 五、完整技术栈总结

| 层次 | 技术 | 说明 |
|------|------|------|
| 托管 | GitHub Pages | 将仓库静态文件发布到公开网址，免费 |
| 部署 | GitHub Actions | 将仓库文件打包并推送到 Pages CDN |
| 页面结构 | HTML + CSS | 写死在 `.html` 文件里，直接渲染 |
| Markdown 渲染 | marked.js（CDN） | 浏览器端把 `.md` 转成 HTML |
| 代码高亮 | highlight.js（CDN） | 浏览器端为代码块添加颜色 |
| 身份验证 | auth.js + sessionStorage | 纯前端密码保护，不依赖服务器 |

---

## 六、常见问题

**Q：我改了 `.md` 文件，为什么网页没变？**  
A：`git push` 后需要手动触发一次部署工作流（或等旧的自动工作流运行完毕），浏览器才能下载到最新版的 `.md` 文件。

**Q：为什么不直接访问 `.md` 文件的网址？**  
A：可以访问，但只会看到纯文本，没有样式。必须通过对应的 `.html` 页面才能看到渲染后的效果。

**Q：为什么 `.html` 文件里要加载 Markdown 而不直接写 HTML？**  
A：Markdown 更易于编写和维护，适合存放大量文档内容；HTML 负责页面布局和交互，两者分工合作。

**Q：网站改动多久生效？**  
A：触发部署工作流后，一般 **1–3 分钟**内生效。若浏览器显示旧页面，按 `Ctrl + Shift + R` 强制刷新。
