╔════════════════════════════════════════════════════════════════════════════╗
║                     QHPhysics 文档外网部署指南                              ║
║                   从本地电脑 → GitHub Pages → 全球可访问                   ║
╚════════════════════════════════════════════════════════════════════════════╝

【重要】部署前准备
═══════════════════════════════════════════════════════════════════════════

✓ 需要：GitHub 账号（你已有）
✓ 需要：Windows 命令行（你已有）
✓ 需要：你的 GitHub 用户名（NOT 邮箱！）


【Step 1】查找你的 GitHub 用户名
═══════════════════════════════════════════════════════════════════════════

重要：GitHub 用户名 ≠ 邮箱地址！

操作步骤：
1. 打开浏览器，登录 https://github.com
2. 点击右上角的头像 → Settings
3. 在 Account 页面或顶部找到你的用户名
   （格式如：john-doe、johndoe123 等）

记住这个用户名，你后面会用到！


【Step 2】运行自动部署脚本
═══════════════════════════════════════════════════════════════════════════

1. 打开命令行（按 Win + R，输入 cmd，回车）

2. 输入这个命令（替换 your_username 为你的 GitHub 用户名）：

   cd F:\new\fishinggame\QHPlugins\QHForUnity
   deploy_to_github.bat your_username

3. 例如，如果用户名是 "john-doe"：
   
   deploy_to_github.bat john-doe

4. 脚本运行时可能要求输入密码
   - 输入你的 GitHub 密码（不会显示，正常情况）
   - 或使用 Personal Access Token（见下面）


【密码问题】如果密码验证失败
═══════════════════════════════════════════════════════════════════════════

GitHub 不再支持密码验证，需要使用 Personal Access Token：

1. 访问 https://github.com/settings/tokens
2. 点击 "Generate new token"
3. 勾选 "repo" 权限
4. 点击 "Generate token"
5. 复制生成的 token（一长串字符）
6. 当脚本要求密码时，粘贴这个 token（Ctrl+V）


【Step 3】启用 GitHub Pages
═══════════════════════════════════════════════════════════════════════════

脚本完成后会显示一个链接，复制并打开它：

https://github.com/your_username/qhphysics-docs/settings/pages

在这个页面：
1. Source 选择 "Deploy from a branch"
2. Branch 选择 "main"
3. Folder 选择 "/ (root)"
4. 点击 Save

等待 1-2 分钟...


【完成！】你的网站链接
═══════════════════════════════════════════════════════════════════════════

部署完成后，你可以在这个地址访问你的文档：

https://your_username.github.io/qhphysics-docs/Documentation_Hub.html

例如：
https://john-doe.github.io/qhphysics-docs/Documentation_Hub.html

现在任何人都可以从浏览器访问你的完整文档了！


【常见问题】
═══════════════════════════════════════════════════════════════════════════

Q: "git not found" 错误
A: 需要安装 Git，下载自 https://git-scm.com/download/win

Q: 密码错误
A: 使用 Personal Access Token（见上面的"密码问题"）

Q: 网站 404 不存在
A: 
  - 检查 GitHub Pages 已启用
  - 等待 3-5 分钟
  - 浏览器硬刷新（Ctrl+Shift+R）

Q: 我用错了用户名怎么办？
A: 重新运行脚本即可

Q: 如何更新网站内容？
A: 修改文件后运行
   git add .
   git commit -m "Update docs"
   git push


【部署文件】
═══════════════════════════════════════════════════════════════════════════

这个文件夹中有：
- deploy_to_github.bat        ← 自动部署脚本（Windows）
- deploy_to_github.sh         ← 自动部署脚本（Mac/Linux）
- GITHUB_DEPLOYMENT_GUIDE.md  ← 完整部署指南
- DEPLOYMENT_VISUAL_GUIDE.md  ← 可视化部署指南


【需要帮助？】
═══════════════════════════════════════════════════════════════════════════

查看这些文件以获得更详细的说明：
- QUICK_DEPLOYMENT_CHECKLIST.md    快速检查清单
- DEPLOYMENT_VISUAL_GUIDE.md       可视化指南（含截图提示）
- GITHUB_DEPLOYMENT_GUIDE.md       完整参考指南


════════════════════════════════════════════════════════════════════════════

准备好了？
1. 查找你的 GitHub 用户名
2. 运行 deploy_to_github.bat your_username
3. 按照提示启用 GitHub Pages
4. 完成！你的网站现在可以从外网访问了

═══════════════════════════════════════════════════════════════════════════
