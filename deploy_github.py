#!/usr/bin/env python3
"""
QHPhysics 文档自动部署到 GitHub Pages
使用方法：python deploy_github.py wangxun111
"""

import os
import sys
import subprocess
import json

def run_command(cmd, description=""):
    """运行命令并显示结果"""
    if description:
        print(f"\n【{description}】")
    print(f"执行: {cmd}\n")

    result = subprocess.run(cmd, shell=True, capture_output=True, text=True)

    if result.stdout:
        print(result.stdout)
    if result.stderr:
        print("⚠️ ", result.stderr)

    return result.returncode == 0

def main():
    # 检查参数
    if len(sys.argv) < 2:
        print("❌ 错误：需要提供 GitHub 用户名")
        print("\n使用方法：")
        print("  python deploy_github.py wangxun111")
        sys.exit(1)

    username = sys.argv[1]
    repo_name = "qhphysics-docs"

    print("=" * 60)
    print("  QHPhysics 文档 GitHub Pages 自动部署")
    print("=" * 60)
    print(f"\n✓ GitHub 用户名: {username}")
    print(f"✓ 仓库名称: {repo_name}")
    print(f"✓ 远程 URL: https://github.com/{username}/{repo_name}.git")
    print()

    # Step 1: 检查 Git
    print("【Step 1】检查 Git")
    result = subprocess.run("git --version", shell=True, capture_output=True, text=True)
    if result.returncode != 0:
        print("❌ 未安装 Git，请先安装")
        print("下载: https://git-scm.com/download/win")
        sys.exit(1)
    print(f"✓ {result.stdout.strip()}")

    # Step 2: 初始化 Git
    if not os.path.exists(".git"):
        print("\n【Step 2】初始化 Git 仓库")
        run_command("git init", "初始化")
    else:
        print("\n【Step 2】Git 仓库已存在")

    # Step 3: 配置 Git
    print("\n【Step 3】配置 Git")
    run_command(f'git config user.name "{username}"', "设置用户名")
    run_command(f'git config user.email "{username}@users.noreply.github.com"', "设置邮箱")

    # Step 4: 添加远程仓库
    print("\n【Step 4】添加 GitHub 远程仓库")

    # 检查是否已存在 origin
    result = subprocess.run("git remote get-url origin", shell=True, capture_output=True, text=True)
    if result.returncode == 0:
        print("移除现有的 origin...")
        run_command("git remote remove origin", "删除旧 origin")

    remote_url = f"https://github.com/{username}/{repo_name}.git"
    run_command(f'git remote add origin "{remote_url}"', "添加新 origin")
    print(f"✓ 远程仓库: {remote_url}")

    # Step 5: 添加文件
    print("\n【Step 5】添加文件到 Git")
    run_command("git add .", "添加文件")

    # Step 6: 提交
    print("\n【Step 6】创建提交")
    result = subprocess.run(
        'git commit -m "docs: Initial commit - QHPhysics documentation hub"',
        shell=True, capture_output=True, text=True
    )
    if "nothing to commit" in result.stdout or "nothing to commit" in result.stderr:
        print("⚠️ 没有需要提交的更改（这是正常的）")
    else:
        print("✓ 提交已创建")

    # Step 7: 推送
    print("\n【Step 7】推送到 GitHub")
    print("⚠️ 此步骤需要输入 GitHub 凭据")
    print("提示：如果用密码验证失败，请使用 Personal Access Token")
    print("访问: https://github.com/settings/tokens")
    print()

    # 先切换分支到 main
    run_command("git branch -M main", "切换分支名为 main")

    # 推送
    result = subprocess.run("git push -u origin main", shell=True)

    if result.returncode == 0:
        print("\n✓ 文件已推送到 GitHub")
    else:
        print("\n⚠️ 推送可能失败，请检查凭据")

    # 完成
    print("\n" + "=" * 60)
    print("✓ 部署步骤完成！")
    print("=" * 60)
    print(f"""
【下一步】启用 GitHub Pages：

1. 打开浏览器，访问：
   https://github.com/{username}/{repo_name}/settings/pages

2. 在 'Source' 中选择：'Deploy from a branch'

3. 选择分支：'main' 和 '/ (root)'

4. 点击 'Save'

【预期的网址】：
主页：
  https://{username}.github.io/{repo_name}/

文档中心：
  https://{username}.github.io/{repo_name}/Documentation_Hub.html

✓ 大约 1-2 分钟后就能访问你的网站！
""")

if __name__ == "__main__":
    main()
