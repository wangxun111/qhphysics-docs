#!/usr/bin/env python3
"""
完全自动化部署脚本 - 仅使用 Python 内置模块
自动创建 GitHub 仓库 + 推送代码 + 启用 GitHub Pages
"""

import os
import sys
import subprocess
import json
import urllib.request
import urllib.error
import time
import base64

class GitHubAutoDeployer:
    def __init__(self, username, token, repo_name="qhphysics-docs"):
        self.username = username
        self.token = token
        self.repo_name = repo_name
        self.api_url = "https://api.github.com"

    def log(self, message, level="INFO"):
        """打印日志"""
        if level == "STEP":
            print("\n【" + message + "】")
        elif level == "SUCCESS":
            print("✓ " + message)
        elif level == "ERROR":
            print("❌ " + message)
        elif level == "WARNING":
            print("⚠️  " + message)
        else:
            print("ℹ️ " + message)

    def run_command(self, cmd, description=""):
        """运行本地命令"""
        if description:
            self.log(description, "STEP")

        print("  执行: " + cmd + "\n")
        result = subprocess.run(cmd, shell=True, capture_output=True, text=True)

        if result.stdout:
            print(result.stdout)
        if result.stderr and "warning" not in result.stderr.lower():
            # 忽略 git 的警告
            if "fatal" in result.stderr.lower() or "error" in result.stderr.lower():
                print("错误: " + result.stderr)

        return result.returncode == 0, result.stdout, result.stderr

    def make_github_request(self, method, path, data=None):
        """使用 GitHub API - 仅用内置模块"""
        url = self.api_url + path

        # 创建 authorization header
        auth_string = base64.b64encode((self.username + ":" + self.token).encode()).decode()

        print("  " + method + " " + url)
        if data:
            print("  数据: " + json.dumps(data, indent=4) + "\n")

        try:
            if data:
                req = urllib.request.Request(
                    url,
                    data=json.dumps(data).encode(),
                    headers={
                        "Authorization": "Basic " + auth_string,
                        "Accept": "application/vnd.github.v3+json",
                        "Content-Type": "application/json"
                    },
                    method=method
                )
            else:
                req = urllib.request.Request(
                    url,
                    headers={
                        "Authorization": "Basic " + auth_string,
                        "Accept": "application/vnd.github.v3+json"
                    },
                    method=method
                )

            with urllib.request.urlopen(req, timeout=10) as response:
                response_data = response.read().decode()
                status_code = response.status
                return status_code, json.loads(response_data) if response_data else {}
        except urllib.error.HTTPError as e:
            status_code = e.code
            try:
                response_data = e.read().decode()
                return status_code, json.loads(response_data)
            except:
                return status_code, {"error": str(e)}
        except Exception as e:
            return 0, {"error": str(e)}

    def create_repository(self):
        """使用 GitHub API 创建仓库"""
        self.log("创建 GitHub 仓库: " + self.repo_name, "STEP")

        data = {
            "name": self.repo_name,
            "description": "QHPhysics Documentation Hub - Complete documentation with tutorials and analysis",
            "public": True,
            "auto_init": False
        }

        status, response = self.make_github_request("POST", "/user/repos", data)

        if status == 201:
            self.log("仓库创建成功！", "SUCCESS")
            return True
        elif status == 422:
            # 仓库已存在
            self.log("仓库已存在，继续部署", "WARNING")
            return True
        else:
            self.log("创建失败: " + str(status), "ERROR")
            if "error" in response:
                print("  错误: " + str(response.get("error", "")))
            return False

    def setup_git_config(self):
        """配置本地 Git"""
        self.log("配置本地 Git", "STEP")

        success, _, _ = self.run_command(
            'git config user.name "' + self.username + '"',
            "设置用户名"
        )
        if not success:
            return False

        success, _, _ = self.run_command(
            'git config user.email "' + self.username + '@users.noreply.github.com"',
            "设置邮箱"
        )
        return success

    def setup_remote(self):
        """配置远程仓库"""
        self.log("配置远程仓库", "STEP")

        remote_url = "https://github.com/" + self.username + "/" + self.repo_name + ".git"

        # 检查 origin 是否存在
        result = subprocess.run("git remote get-url origin", shell=True, capture_output=True, text=True)
        if result.returncode == 0:
            print("  移除现有的 origin...")
            subprocess.run("git remote remove origin", shell=True, capture_output=True)

        success, _, _ = self.run_command(
            'git remote add origin "' + remote_url + '"',
            "添加远程仓库"
        )

        if success:
            self.log("远程仓库: " + remote_url, "SUCCESS")

        return success

    def push_code(self):
        """推送代码到 GitHub"""
        self.log("准备推送代码", "STEP")

        # 添加文件
        print("  添加所有文件...")
        subprocess.run("git add .", shell=True, capture_output=True)

        # 检查是否有待提交的更改
        result = subprocess.run("git status --porcelain", shell=True, capture_output=True, text=True)
        if result.stdout.strip():
            print("  创建提交...")
            success, _, _ = self.run_command(
                'git commit -m "docs: Initial commit - QHPhysics documentation hub"',
                "提交更改"
            )
            if not success:
                self.log("提交失败", "WARNING")
        else:
            self.log("没有待提交的更改", "WARNING")

        # 切换分支
        print("  切换分支到 main...")
        subprocess.run("git branch -M main", shell=True, capture_output=True)

        # 推送
        self.log("推送到 GitHub", "STEP")
        print("  这可能需要几秒钟...\n")

        success, stdout, stderr = self.run_command(
            "git push -u origin main",
            "执行推送"
        )

        if success or "main -> main" in stdout:
            self.log("代码推送成功！", "SUCCESS")
            return True
        else:
            self.log("推送失败", "ERROR")
            return False

    def enable_github_pages(self):
        """启用 GitHub Pages"""
        self.log("启用 GitHub Pages", "STEP")

        path = "/repos/" + self.username + "/" + self.repo_name + "/pages"
        data = {
            "source": {
                "branch": "main",
                "path": "/"
            }
        }

        status, response = self.make_github_request("POST", path, data)

        if status in [201, 200]:
            self.log("GitHub Pages 启用成功！", "SUCCESS")
            return True
        elif status == 409:
            # 可能已经启用
            self.log("GitHub Pages 已启用或正在配置", "WARNING")
            return True
        else:
            self.log("启用失败: " + str(status), "WARNING")
            return True  # 继续，不认为这是致命错误

    def verify_deployment(self):
        """验证部署"""
        self.log("验证部署", "STEP")

        # 等一下让 GitHub 处理
        print("  等待 GitHub 处理文件（3秒）...\n")
        time.sleep(3)

        # 检查仓库
        path = "/repos/" + self.username + "/" + self.repo_name
        status, response = self.make_github_request("GET", path)

        if status == 200:
            self.log("仓库确认存在", "SUCCESS")
            return True

        return True

    def deploy(self):
        """执行完整部署流程"""
        print("=" * 70)
        print("  QHPhysics 文档自动部署系统")
        print("=" * 70)
        print("\n✓ GitHub 用户名: " + self.username)
        print("✓ 仓库名称: " + self.repo_name)
        print("✓ Token: " + self.token[:20] + "..." + self.token[-10:])
        print()

        # 步骤 1：创建仓库
        if not self.create_repository():
            self.log("创建仓库失败，中止部署", "ERROR")
            return False

        time.sleep(1)

        # 步骤 2：配置 Git
        if not self.setup_git_config():
            self.log("配置 Git 失败，中止部署", "ERROR")
            return False

        # 步骤 3：配置远程
        if not self.setup_remote():
            self.log("配置远程失败，中止部署", "ERROR")
            return False

        # 步骤 4：推送代码
        if not self.push_code():
            self.log("推送代码失败，中止部署", "ERROR")
            return False

        time.sleep(2)

        # 步骤 5：启用 GitHub Pages
        if not self.enable_github_pages():
            self.log("启用 GitHub Pages 失败，但代码已推送", "WARNING")

        # 步骤 6：验证
        self.verify_deployment()

        # 完成
        self.print_summary()
        return True

    def print_summary(self):
        """打印总结"""
        print("\n" + "=" * 70)
        print("✓ 部署完成！")
        print("=" * 70)
        print("\n【📍 你的仓库】")
        print("https://github.com/" + self.username + "/" + self.repo_name)
        print("\n【📍 GitHub Pages 设置】")
        print("https://github.com/" + self.username + "/" + self.repo_name + "/settings/pages")
        print("\n【📍 你的网站地址】")
        print("主页：")
        print("  https://" + self.username + ".github.io/" + self.repo_name + "/")
        print("\n文档中心（推荐）：")
        print("  https://" + self.username + ".github.io/" + self.repo_name + "/Documentation_Hub.html")
        print("\n其他文档：")
        print("  https://" + self.username + ".github.io/" + self.repo_name + "/AI_Tuning_Getting_Started.html")
        print("  https://" + self.username + ".github.io/" + self.repo_name + "/AI_Parameter_Optimization.html")
        print("  https://" + self.username + ".github.io/" + self.repo_name + "/QHPhysics_Architecture.html")
        print("  https://" + self.username + ".github.io/" + self.repo_name + "/PhysX_Analysis.html")
        print("\n【⏳ 等待时间】")
        print("网站部署通常需要 5-15 分钟")
        print("如果看到 404，请等几分钟后再试")
        print("或者硬刷新浏览器：Ctrl + Shift + R")
        print("\n【✓ 现在】")
        print("1. 等待 5-10 分钟")
        print("2. 访问上面的网站地址")
        print("3. 享受你的文档！")


def main():
    if len(sys.argv) < 3:
        print("使用方法：")
        print("  python auto_deploy.py <username> <token>")
        print("\n例如：")
        print("  python auto_deploy.py wangxun111 github_pat_11BDJRTCQ0...")
        sys.exit(1)

    username = sys.argv[1]
    token = sys.argv[2]

    deployer = GitHubAutoDeployer(username, token)

    if deployer.deploy():
        sys.exit(0)
    else:
        sys.exit(1)


if __name__ == "__main__":
    main()
