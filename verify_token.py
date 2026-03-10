#!/usr/bin/env python3
"""
验证 GitHub Token 是否有效
"""

import sys
import urllib.request
import urllib.error
import json
import base64

def verify_token(username, token):
    """验证 token 是否有效"""
    print("验证 GitHub Token...")
    print("用户名: " + username)
    print("Token: " + token[:20] + "..." + token[-10:])
    print()

    # 尝试获取用户信息
    url = "https://api.github.com/user"

    try:
        # 使用 token 作为密码的 Basic Auth
        auth_string = base64.b64encode((username + ":" + token).encode()).decode()

        req = urllib.request.Request(
            url,
            headers={
                "Authorization": "Basic " + auth_string,
                "Accept": "application/vnd.github.v3+json"
            }
        )

        with urllib.request.urlopen(req, timeout=10) as response:
            data = json.loads(response.read().decode())

            print("✓ Token 有效！")
            print()
            print("用户信息:")
            print("  登录: " + data.get("login", "N/A"))
            print("  名称: " + data.get("name", "N/A"))
            print("  公开仓库: " + str(data.get("public_repos", 0)))
            print()

            # 检查作用域
            print("Token 作用域:")
            if "X-OAuth-Scopes" in response.headers:
                scopes = response.headers["X-OAuth-Scopes"]
                print("  " + scopes if scopes else "  (无作用域)")
            else:
                print("  (无法获取作用域信息)")

            return True

    except urllib.error.HTTPError as e:
        print("❌ Token 验证失败！")
        print("错误代码: " + str(e.code))

        try:
            error_data = json.loads(e.read().decode())
            print("错误信息: " + str(error_data.get("message", "")))
        except:
            pass

        print()
        print("可能的原因：")
        if e.code == 401:
            print("  1. Token 无效或已过期")
            print("  2. Token 格式错误")
        elif e.code == 403:
            print("  1. Token 权限不足")
            print("  2. Token 已被撤销")
            print("  3. API 速率限制")

        print()
        print("解决方案：")
        print("  1. 访问: https://github.com/settings/tokens")
        print("  2. 生成新的 Personal Access Token")
        print("  3. 勾选 'repo' 权限")
        print("  4. 复制新 Token 重试")

        return False

    except Exception as e:
        print("❌ 请求失败: " + str(e))
        return False


if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("使用方法:")
        print("  python verify_token.py <username> <token>")
        sys.exit(1)

    username = sys.argv[1]
    token = sys.argv[2]

    if verify_token(username, token):
        sys.exit(0)
    else:
        sys.exit(1)
