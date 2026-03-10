#!/bin/bash
# QHPhysics 文档部署到 GitHub Pages 脚本
# 使用方法：bash deploy_to_github.sh <github_username> <repository_name>

set -e

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}  QHPhysics 文档 GitHub Pages 部署脚本${NC}"
echo -e "${BLUE}========================================${NC}"

# 获取参数
GITHUB_USERNAME=${1:-""}
REPO_NAME=${2:-"qhphysics-docs"}

if [ -z "$GITHUB_USERNAME" ]; then
    echo -e "${RED}❌ 错误：需要提供 GitHub 用户名${NC}"
    echo -e "${YELLOW}使用方法：${NC}"
    echo "  bash deploy_to_github.sh <github_username> [repository_name]"
    echo ""
    echo "示例："
    echo "  bash deploy_to_github.sh octocat"
    echo "  bash deploy_to_github.sh octocat my-docs"
    exit 1
fi

echo -e "${GREEN}✓ GitHub 用户名：${NC}$GITHUB_USERNAME"
echo -e "${GREEN}✓ 仓库名称：${NC}$REPO_NAME"
echo ""

# Step 1: 检查 git
echo -e "${BLUE}【Step 1】检查 Git 安装${NC}"
if ! command -v git &> /dev/null; then
    echo -e "${RED}❌ 未找到 Git，请先安装 Git${NC}"
    echo "下载地址：https://git-scm.com/download/win"
    exit 1
fi
echo -e "${GREEN}✓ Git 已安装${NC}"
echo ""

# Step 2: 初始化 Git 仓库（如果需要）
echo -e "${BLUE}【Step 2】初始化 Git 仓库${NC}"
if [ ! -d ".git" ]; then
    echo "初始化新的 Git 仓库..."
    git init
    echo -e "${GREEN}✓ Git 仓库初始化完成${NC}"
else
    echo -e "${GREEN}✓ Git 仓库已存在${NC}"
fi
echo ""

# Step 3: 配置 Git
echo -e "${BLUE}【Step 3】配置 Git${NC}"
git config user.name "$GITHUB_USERNAME" || git config --global user.name "$GITHUB_USERNAME"
git config user.email "$GITHUB_USERNAME@users.noreply.github.com" || git config --global user.email "$GITHUB_USERNAME@users.noreply.github.com"
echo -e "${GREEN}✓ Git 配置完成${NC}"
echo ""

# Step 4: 添加远程仓库
echo -e "${BLUE}【Step 4】添加 GitHub 远程仓库${NC}"
REMOTE_URL="https://github.com/$GITHUB_USERNAME/$REPO_NAME.git"
echo "远程仓库 URL：$REMOTE_URL"

if git remote get-url origin &> /dev/null; then
    echo "移除现有的 origin..."
    git remote remove origin
fi

git remote add origin "$REMOTE_URL"
echo -e "${GREEN}✓ 远程仓库已配置${NC}"
echo ""

# Step 5: 创建 .gitignore（可选）
echo -e "${BLUE}【Step 5】创建 .gitignore${NC}"
if [ ! -f ".gitignore" ]; then
    cat > .gitignore << 'EOF'
# IDEs
.vscode/
.idea/
*.swp
*.swo

# OS
.DS_Store
Thumbs.db

# Build
node_modules/
dist/
build/
EOF
    echo -e "${GREEN}✓ .gitignore 已创建${NC}"
else
    echo -e "${GREEN}✓ .gitignore 已存在${NC}"
fi
echo ""

# Step 6: 添加所有文件
echo -e "${BLUE}【Step 6】添加文件到 Git${NC}"
echo "添加所有文件..."
git add .
echo -e "${GREEN}✓ 文件已添加${NC}"
echo ""

# Step 7: 创建提交
echo -e "${BLUE}【Step 7】创建初始提交${NC}"
if ! git diff-index --quiet HEAD --; then
    git commit -m "docs: Initial commit - QHPhysics documentation hub"
    echo -e "${GREEN}✓ 提交已创建${NC}"
else
    echo -e "${YELLOW}⚠ 没有需要提交的更改${NC}"
fi
echo ""

# Step 8: 推送到 GitHub
echo -e "${BLUE}【Step 8】推送到 GitHub${NC}"
echo -e "${YELLOW}⚠ 此步骤需要你的 GitHub 凭据${NC}"
echo "首次推送需要认证。如果出现提示，请输入你的 GitHub 用户名和密码（或 Personal Access Token）"
echo ""

git branch -M main
git push -u origin main

echo -e "${GREEN}✓ 文件已推送到 GitHub${NC}"
echo ""

# Step 9: 提示启用 GitHub Pages
echo -e "${BLUE}========================================${NC}"
echo -e "${GREEN}✓ 部署步骤完成！${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""
echo -e "${YELLOW}【下一步】启用 GitHub Pages：${NC}"
echo ""
echo "1. 打开浏览器，访问："
echo "   https://github.com/$GITHUB_USERNAME/$REPO_NAME/settings/pages"
echo ""
echo "2. 在 'Source' 中选择：'Deploy from a branch'"
echo ""
echo "3. 选择分支：'main' 和 '/ (root)'"
echo ""
echo "4. 点击 'Save'"
echo ""
echo -e "${YELLOW}【预期的网址】：${NC}"
echo "主页："
echo "  https://$GITHUB_USERNAME.github.io/$REPO_NAME/"
echo ""
echo "文档中心："
echo "  https://$GITHUB_USERNAME.github.io/$REPO_NAME/Documentation_Hub.html"
echo ""
echo -e "${GREEN}✓ 大约 1-2 分钟后就能访问你的网站！${NC}"
echo ""
