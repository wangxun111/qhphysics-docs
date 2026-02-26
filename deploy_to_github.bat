@echo off
REM QHPhysics 文档部署到 GitHub Pages 脚本（Windows版本）
REM 使用方法：deploy_to_github.bat <github_username>

setlocal enabledelayedexpansion

echo.
echo ========================================
echo   QHPhysics 文档 GitHub Pages 部署脚本
echo ========================================
echo.

REM 获取参数
set GITHUB_USERNAME=%1
set REPO_NAME=%2

if "%REPO_NAME%"=="" (
    set REPO_NAME=qhphysics-docs
)

if "%GITHUB_USERNAME%"=="" (
    echo ❌ 错误：需要提供 GitHub 用户名
    echo.
    echo 使用方法：
    echo   deploy_to_github.bat ^<github_username^> [repository_name]
    echo.
    echo 示例：
    echo   deploy_to_github.bat octocat
    echo   deploy_to_github.bat octocat my-docs
    echo.
    pause
    exit /b 1
)

echo ✓ GitHub 用户名：%GITHUB_USERNAME%
echo ✓ 仓库名称：%REPO_NAME%
echo.

REM Step 1: 检查 git
echo 【Step 1】检查 Git 安装
where git >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo ❌ 未找到 Git，请先安装 Git
    echo 下载地址：https://git-scm.com/download/win
    pause
    exit /b 1
)
echo ✓ Git 已安装
echo.

REM Step 2: 初始化 Git 仓库
echo 【Step 2】初始化 Git 仓库
if not exist ".git" (
    echo 初始化新的 Git 仓库...
    call git init
    echo ✓ Git 仓库初始化完成
) else (
    echo ✓ Git 仓库已存在
)
echo.

REM Step 3: 配置 Git
echo 【Step 3】配置 Git
call git config user.name "%GITHUB_USERNAME%"
call git config user.email "%GITHUB_USERNAME%@users.noreply.github.com"
echo ✓ Git 配置完成
echo.

REM Step 4: 添加远程仓库
echo 【Step 4】添加 GitHub 远程仓库
set REMOTE_URL=https://github.com/%GITHUB_USERNAME%/%REPO_NAME%.git
echo 远程仓库 URL：%REMOTE_URL%

call git remote get-url origin >nul 2>nul
if %ERRORLEVEL% equ 0 (
    echo 移除现有的 origin...
    call git remote remove origin
)

call git remote add origin "%REMOTE_URL%"
echo ✓ 远程仓库已配置
echo.

REM Step 5: 创建 .gitignore
echo 【Step 5】创建 .gitignore
if not exist ".gitignore" (
    (
        echo # IDEs
        echo .vscode/
        echo .idea/
        echo *.swp
        echo *.swo
        echo.
        echo # OS
        echo .DS_Store
        echo Thumbs.db
        echo.
        echo # Build
        echo node_modules/
        echo dist/
        echo build/
    ) > .gitignore
    echo ✓ .gitignore 已创建
) else (
    echo ✓ .gitignore 已存在
)
echo.

REM Step 6: 添加所有文件
echo 【Step 6】添加文件到 Git
echo 添加所有文件...
call git add .
echo ✓ 文件已添加
echo.

REM Step 7: 创建提交
echo 【Step 7】创建初始提交
call git commit -m "docs: Initial commit - QHPhysics documentation hub" >nul 2>nul
if %ERRORLEVEL% equ 0 (
    echo ✓ 提交已创建
) else (
    echo ⚠ 没有需要提交的更改或提交失败
)
echo.

REM Step 8: 推送到 GitHub
echo 【Step 8】推送到 GitHub
echo ⚠ 此步骤需要你的 GitHub 凭据
echo 首次推送需要认证。如果出现提示，请输入你的 GitHub 用户名和密码（或 Personal Access Token）
echo.

call git branch -M main >nul 2>nul
call git push -u origin main

if %ERRORLEVEL% equ 0 (
    echo ✓ 文件已推送到 GitHub
) else (
    echo ❌ 推送失败！请检查网络连接和 GitHub 凭据
    pause
    exit /b 1
)
echo.

REM Step 9: 完成提示
echo ========================================
echo ✓ 部署步骤完成！
echo ========================================
echo.
echo 【下一步】启用 GitHub Pages：
echo.
echo 1. 打开浏览器，访问：
echo    https://github.com/%GITHUB_USERNAME%/%REPO_NAME%/settings/pages
echo.
echo 2. 在 'Source' 中选择：'Deploy from a branch'
echo.
echo 3. 选择分支：'main' 和 '/ (root)'
echo.
echo 4. 点击 'Save'
echo.
echo 【预期的网址】：
echo 主页：
echo   https://%GITHUB_USERNAME%.github.io/%REPO_NAME%/
echo.
echo 文档中心：
echo   https://%GITHUB_USERNAME%.github.io/%REPO_NAME%/Documentation_Hub.html
echo.
echo ✓ 大约 1-2 分钟后就能访问你的网站！
echo.
pause
