# 🚀 针对 wangxun111 的个性化部署指南

## 你的部署信息

| 信息 | 内容 |
|------|------|
| GitHub 用户名 | `wangxun111` |
| 仓库名称 | `qhphysics-docs` |
| 部署方式 | GitHub Pages |
| 预期网址 | https://wangxun111.github.io/qhphysics-docs/ |

---

## ⚡ 快速部署（3分钟搞定）

### Step 1: 打开命令行

```bash
按 Win + R → 输入 cmd → 回车
```

### Step 2: 进入文档目录并运行部署脚本

**复制并粘贴这条完整命令**：

```bash
cd F:\new\fishinggame\QHPlugins\QHForUnity && deploy_to_github.bat wangxun111
```

然后按回车。

### Step 3: 输入密码或 Token

脚本运行时会要求输入密码：

```
Username for 'https://github.com': wangxun111
Password for 'https://wangxun111@github.com': _
```

**选项 A：直接输入 GitHub 密码**
- 输入你的 GitHub 账户密码
- 密码不会显示（正常现象），输入完后回车

**选项 B：使用 Personal Access Token（推荐）**
1. 访问 https://github.com/settings/tokens
2. 点击 "Generate new token"
3. 勾选 "repo" 权限
4. 点击 "Generate token"
5. 复制生成的 token
6. 粘贴到密码框（Ctrl+V）

### Step 4: 启用 GitHub Pages

脚本运行完成后会看到这样的提示：

```
【下一步】启用 GitHub Pages：

1. 打开浏览器，访问：
   https://github.com/wangxun111/qhphysics-docs/settings/pages

2. 在 'Source' 中选择：'Deploy from a branch'

3. 选择分支：'main' 和 '/ (root)'

4. 点击 'Save'
```

**打开这个链接**（复制粘贴到浏览器）：
```
https://github.com/wangxun111/qhphysics-docs/settings/pages
```

在这个页面：
1. 找到 "Source" 部分
2. 选择 "Deploy from a branch"
3. 选择分支：**main**
4. 选择文件夹：**/ (root)**
5. 点击 **Save**

---

## ✅ 完成！你的网站链接

部署完成后（1-2分钟），你可以访问：

### 📍 主页（文档中心）
```
https://wangxun111.github.io/qhphysics-docs/
```

### 📍 所有文档

**文档中心导航页**：
```
https://wangxun111.github.io/qhphysics-docs/Documentation_Hub.html
```

**AI参数调优入门教程**：
```
https://wangxun111.github.io/qhphysics-docs/AI_Tuning_Getting_Started.html
```

**AI参数调优深度分析**：
```
https://wangxun111.github.io/qhphysics-docs/AI_Parameter_Optimization.html
```

**XunPhysics 完整架构**：
```
https://wangxun111.github.io/qhphysics-docs/XunPhysics_Architecture.html
```

**PhysX 深度分析**：
```
https://wangxun111.github.io/qhphysics-docs/PhysX_Analysis.html
```

---

## 🔍 验证部署是否成功

### 检查 1: 仓库是否创建

打开：
```
https://github.com/wangxun111/qhphysics-docs
```

你应该看到：
- ✅ 仓库名：qhphysics-docs
- ✅ 分支：main
- ✅ 文件列表（所有文档）

### 检查 2: GitHub Pages 是否启用

打开：
```
https://github.com/wangxun111/qhphysics-docs/settings/pages
```

你应该看到：
- ✅ Source: Deploy from a branch
- ✅ Branch: main / (root)
- ✅ 状态：Your site is published at https://wangxun111.github.io/qhphysics-docs/

### 检查 3: 网站是否可访问

打开：
```
https://wangxun111.github.io/qhphysics-docs/Documentation_Hub.html
```

你应该看到：
- ✅ 文档中心页面正常显示
- ✅ 所有链接可点击
- ✅ 样式正常加载

---

## ⏱️ 时间表

| 时间 | 事件 |
|------|------|
| 0分钟 | 运行部署脚本 |
| 1-2分钟 | 脚本完成，文件上传到 GitHub |
| 3-5分钟 | GitHub Pages 开始构建（可能有延迟） |
| 5-10分钟 | 网站部署完成，可以访问 |

**第一次访问可能需要等待 5-10 分钟，这是正常的。**

---

## 🔧 故障排除

### 问题 1: "git not found" 错误

**解决**：
1. 下载 Git：https://git-scm.com/download/win
2. 安装 Git
3. 重新运行脚本

### 问题 2: 密码验证失败

**解决**：
1. 访问 https://github.com/settings/tokens
2. 创建新 Personal Access Token
3. 勾选 "repo" 权限
4. 复制 Token
5. 重新运行脚本，使用 Token 作为密码

### 问题 3: 网站仍然 404

**解决**：
- 确保已启用 GitHub Pages（Settings → Pages）
- 等待 5-10 分钟
- 在浏览器中硬刷新：Ctrl + Shift + R
- 检查网址是否正确

### 问题 4: 脚本说"远程仓库已存在"

**解决**：
正常情况。脚本会自动处理，继续运行即可。

---

## 📤 后续如何更新网站

修改文档后，只需运行：

```bash
cd F:\new\fishinggame\QHPlugins\QHForUnity

git add .
git commit -m "docs: Update - [你的更改描述]"
git push
```

1-2 分钟后网站会自动更新！

---

## 🔗 分享你的网站

现在可以分享这个链接给任何人：

```
https://wangxun111.github.io/qhphysics-docs/Documentation_Hub.html
```

他们可以在任何地方、任何设备上访问你的完整文档！

---

## 📋 最终检查清单

部署前：
- [ ] 你有 GitHub 账号（已确认）
- [ ] 你的用户名是 wangxun111（已确认）
- [ ] 你有命令行访问权限

部署中：
- [ ] 打开命令行
- [ ] 进入 F:\new\fishinggame\QHPlugins\QHForUnity
- [ ] 运行 `deploy_to_github.bat wangxun111`
- [ ] 输入密码或 Token
- [ ] 等待脚本完成

部署后：
- [ ] 打开 GitHub Pages 设置链接
- [ ] 配置 Source → Deploy from a branch
- [ ] 选择 main 分支和 / (root) 文件夹
- [ ] 点击 Save
- [ ] 等待 5-10 分钟

验证：
- [ ] 访问 https://wangxun111.github.io/qhphysics-docs/
- [ ] 网站正常显示
- [ ] 所有链接可点击

---

## 🎉 现在就开始！

**命令（复制粘贴）**：
```
cd F:\new\fishinggame\QHPlugins\QHForUnity && deploy_to_github.bat wangxun111
```

**最终网址**：
```
https://wangxun111.github.io/qhphysics-docs/Documentation_Hub.html
```

祝部署顺利！如有任何问题，告诉我错误信息，我来帮你解决。

---

**需要帮助？** 查看这些文件：
- DEPLOYMENT_README.txt - 快速指南
- DEPLOYMENT_VISUAL_GUIDE.md - 可视化步骤
- GITHUB_DEPLOYMENT_GUIDE.md - 完整参考
