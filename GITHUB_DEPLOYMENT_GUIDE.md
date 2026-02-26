# GitHub Pages 部署指南

## 快速部署（推荐）

### Windows 用户

**第1步：查找你的 GitHub 用户名**

1. 登录 GitHub：https://github.com
2. 点击右上角头像
3. 选择 "Settings"
4. 在 Account 页面左侧看到你的用户名

**第2步：运行部署脚本**

打开命令行（Win + R，输入 cmd），然后：

```bash
cd F:\new\fishinggame\QHPlugins\QHForUnity

REM 运行部署脚本，替换 your_github_username 为你的实际用户名
deploy_to_github.bat your_github_username
```

例如：
```bash
deploy_to_github.bat octocat
deploy_to_github.bat john-doe
```

**第3步：输入 GitHub 凭据**

脚本运行时会提示输入用户名和密码。有两种选择：

**选项 A：使用密码（推荐）**
```
Username: your_github_username
Password: your_github_password
```

**选项 B：使用 Personal Access Token（推荐用于安全）**
1. 访问 https://github.com/settings/tokens
2. 点击 "Generate new token"
3. 选择权限：repo（完整控制私有仓库）
4. 复制生成的 token
5. 当脚本要求密码时，粘贴 token

**第4步：启用 GitHub Pages**

脚本完成后会显示一个链接，打开它：

```
https://github.com/your_github_username/qhphysics-docs/settings/pages
```

然后：
1. 找到 "Source" 部分
2. 选择 "Deploy from a branch"
3. 选择分支：**main**
4. 选择文件夹：**/ (root)**
5. 点击 "Save"

**完成！** 1-2 分钟后就能访问：
```
https://your_github_username.github.io/qhphysics-docs/Documentation_Hub.html
```

---

### macOS/Linux 用户

```bash
cd F:\new\fishinggame\QHPlugins\QHForUnity

# 运行部署脚本
bash deploy_to_github.sh your_github_username
```

---

## 手动部署（如果脚本不工作）

### Step 1: 初始化 Git 仓库

```bash
cd F:\new\fishinggame\QHPlugins\QHForUnity

git init
git config user.name "your_github_username"
git config user.email "your_email@example.com"
```

### Step 2: 添加 GitHub 远程仓库

```bash
# 替换 your_github_username 为你的用户名
git remote add origin https://github.com/your_github_username/qhphysics-docs.git
```

### Step 3: 添加文件并提交

```bash
git add .
git commit -m "docs: Initial commit - QHPhysics documentation hub"
```

### Step 4: 推送到 GitHub

```bash
git branch -M main
git push -u origin main
```

### Step 5: 启用 GitHub Pages

1. 打开 GitHub 仓库：`https://github.com/your_github_username/qhphysics-docs`
2. 进入 Settings → Pages
3. Source 选择 "Deploy from a branch"
4. 选择 "main" 分支和 "/ (root)" 文件夹
5. 点击 Save

---

## 访问你的网站

部署完成后，你可以通过以下地址访问：

**主页（文档中心）**：
```
https://your_github_username.github.io/qhphysics-docs/
```

**文档中心**：
```
https://your_github_username.github.io/qhphysics-docs/Documentation_Hub.html
```

**其他文档**：
```
https://your_github_username.github.io/qhphysics-docs/QHPhysics_Architecture.html
https://your_github_username.github.io/qhphysics-docs/PhysX_Analysis.html
https://your_github_username.github.io/qhphysics-docs/AI_Tuning_Getting_Started.html
https://your_github_username.github.io/qhphysics-docs/AI_Parameter_Optimization.html
```

---

## 常见问题

### Q1: 我忘记了 GitHub 用户名

**A**:
1. 登录 GitHub
2. 点击右上角头像
3. 选择 Settings
4. 你会看到 "Public profile" 中的用户名

### Q2: 推送时说 "Authentication failed"

**A**:
1. GitHub 不再支持密码验证
2. 使用 Personal Access Token 代替：
   - 访问 https://github.com/settings/tokens
   - 创建新 token（勾选 repo）
   - 用 token 替代密码

### Q3: 网站仍然不能访问

**A**:
1. 确保已启用 GitHub Pages
2. 等待 1-2 分钟再试
3. 在 Settings → Pages 检查部署状态
4. 可能出现的消息：
   - "Your site is ready to be published"：还需等待
   - "Your site is published"：已完成，可以访问

### Q4: 我想用自定义域名

**A**:
1. 在 GitHub Pages 设置中找到 "Custom domain"
2. 输入你的域名（如 docs.example.com）
3. 在域名服务商那里添加 CNAME 记录：
   ```
   your_github_username.github.io
   ```
4. GitHub 会自动配置 HTTPS

### Q5: 如何更新网站内容？

**A**:
1. 修改本地文件
2. 运行：
   ```bash
   git add .
   git commit -m "docs: Update documentation"
   git push
   ```
3. 等待 1-2 分钟，网站会自动更新

---

## 后续更新文档

**每次添加新文档时**：

1. 将新文件放在 `F:\new\fishinggame\QHPlugins\QHForUnity` 目录
2. 运行：
   ```bash
   git add .
   git commit -m "docs: Add new documentation - [文档名]"
   git push
   ```
3. 1-2 分钟后网站自动更新

---

## 分享你的网站

现在你可以分享这个链接：
```
https://your_github_username.github.io/qhphysics-docs/Documentation_Hub.html
```

任何人都可以从浏览器访问你的完整文档！

---

**需要帮助？** 查看 GitHub Pages 官方文档：https://pages.github.com/
