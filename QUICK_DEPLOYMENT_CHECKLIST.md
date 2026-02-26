# 🚀 快速部署清单

## 你的部署信息

请你先完成这一步，然后告诉我你的GitHub用户名：

**查找你的GitHub用户名的方法**：
1. 登录 GitHub：https://github.com
2. 点击右上角头像
3. 选择 "Settings"
4. 在页面左侧或顶部找到你的用户名
   - 格式通常是：`username`（不是邮箱）
   - 示例：`octocat`、`john-doe` 等

---

## 部署步骤（3步搞定）

### 🟢 Step 1: 获取你的GitHub用户名

告诉我你的GitHub用户名（格式：字母+数字+连字符，不是邮箱）

### 🟡 Step 2: 运行部署脚本

在你的电脑上：
1. 打开命令行（Win + R → 输入 cmd）
2. 输入命令：
```bash
cd F:\new\fishinggame\QHPlugins\QHForUnity
deploy_to_github.bat YOUR_GITHUB_USERNAME
```

例如：
```bash
deploy_to_github.bat john-doe
```

3. 按回车，脚本会自动做以下事情：
   ✓ 初始化 Git 仓库
   ✓ 配置 GitHub
   ✓ 上传所有文件
   ✓ 推送到 GitHub

### 🔴 Step 3: 启用 GitHub Pages

脚本完成后会给你一个链接，打开它：
```
https://github.com/YOUR_USERNAME/qhphysics-docs/settings/pages
```

然后：
1. Source 选择 "Deploy from a branch"
2. 选择分支：**main**
3. 选择文件夹：**/ (root)**
4. 点击 Save

---

## ✅ 完成！

等待 1-2 分钟，你的网站就可以在这里访问：

```
https://YOUR_USERNAME.github.io/qhphysics-docs/Documentation_Hub.html
```

---

## 📋 故障排除

### 问题：提示"认证失败"

**解决**：使用 Personal Access Token
1. 访问：https://github.com/settings/tokens
2. 点击 "Generate new token"
3. 勾选 "repo" 权限
4. 复制 token
5. 当脚本要求密码时，粘贴这个 token

### 问题：网站仍然不能访问

**检查清单**：
- [ ] GitHub Pages 已启用
- [ ] 分支选择为 main
- [ ] 文件夹选择为 / (root)
- [ ] 已等待 1-2 分钟
- [ ] 浏览器已刷新（Ctrl+F5）

---

## 📱 分享你的网站

部署完成后，你可以：

✅ 分享这个链接给任何人：
```
https://YOUR_USERNAME.github.io/qhphysics-docs/
```

✅ 所有人都能访问你的完整文档

✅ 随时可以更新（git push 即可）

---

**现在，请告诉我你的 GitHub 用户名，我可以帮你验证部署是否成功！**
