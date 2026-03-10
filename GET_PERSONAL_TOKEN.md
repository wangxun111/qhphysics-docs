# 🔑 获取 Personal Access Token（推荐方式）

## 步骤 1：访问 GitHub 设置

打开浏览器，访问：
```
https://github.com/settings/tokens
```

## 步骤 2：创建新 Token

1. 点击 **"Generate new token"** 按钮
2. 选择 **"Generate new token (classic)"**

## 步骤 3：配置权限

填写表单：
- **Token name**: `qhphysics-deploy` 或任意名称
- **Expiration**: `No expiration` 或选择合适的期限
- **Select scopes**: 勾选 **"repo"**（这样就足够了）

## 步骤 4：生成 Token

点击下面的 **"Generate token"** 按钮

## 步骤 5：复制 Token

✅ 一个长字符串会出现（一次性显示）
- 复制这个字符串（Ctrl+C）
- 保存到某个地方（以后如果忘记会需要重新生成）

## 步骤 6：在命令行中使用

当脚本要求输入用户名和密码时：

```
Username for 'https://github.com': wangxun111
Password for 'https://wangxun111@github.com': [粘贴 Token，Ctrl+V]
```

**注意**：粘贴时密码不会显示（正常现象），直接粘贴后按回车

完成！Token 用于身份验证。
