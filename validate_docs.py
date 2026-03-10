#!/usr/bin/env python3
"""
文档自测脚本 - 检查常见的文档问题
"""

import os
import re
import sys
from pathlib import Path

# 颜色输出
class Colors:
    GREEN = '\033[92m'
    RED = '\033[91m'
    YELLOW = '\033[93m'
    BLUE = '\033[94m'
    END = '\033[0m'

def print_error(msg):
    print(f"{Colors.RED}❌ {msg}{Colors.END}")

def print_warning(msg):
    print(f"{Colors.YELLOW}⚠️  {msg}{Colors.END}")

def print_success(msg):
    print(f"{Colors.GREEN}✅ {msg}{Colors.END}")

def print_info(msg):
    print(f"{Colors.BLUE}ℹ️  {msg}{Colors.END}")

class DocValidator:
    def __init__(self, base_path='.'):
        self.base_path = Path(base_path)
        self.errors = []
        self.warnings = []

    def validate_all(self):
        """运行所有检查"""
        print(f"\n{Colors.BLUE}🔍 开始文档自测...{Colors.END}\n")

        self.check_html_links()
        self.check_modal_visibility()
        self.check_file_names()
        self.check_markdown_references()

        self.print_summary()

        # 返回是否有错误（用于 git hook）
        return len(self.errors) == 0

    def check_html_links(self):
        """检查 HTML 文件中的链接是否指向存在的文件"""
        print_info("检查 HTML 文件中的链接...")

        html_files = list(self.base_path.glob('*.html'))

        for html_file in html_files:
            try:
                with open(html_file, 'r', encoding='utf-8') as f:
                    content = f.read()

                # 查找所有 href 属性
                links = re.findall(r'href=["\'](.*?)["\']', content)

                for link in links:
                    # 跳过外部链接和 # 链接
                    if link.startswith('http') or link.startswith('#'):
                        continue

                    # 解析文件路径
                    file_path = self.base_path / link

                    if not file_path.exists():
                        self.errors.append(
                            f"[{html_file.name}] 链接指向不存在的文件: {link}"
                        )
                    else:
                        print_success(f"{html_file.name}: 链接正常 {link}")

            except Exception as e:
                self.errors.append(f"[{html_file.name}] 读取失败: {str(e)}")

    def check_modal_visibility(self):
        """检查 modal 和 overlay 是否正确隐藏"""
        print_info("\n检查 modal/overlay 是否正确隐藏...")

        html_files = list(self.base_path.glob('*.html'))
        modal_issues = {}

        for html_file in html_files:
            try:
                with open(html_file, 'r', encoding='utf-8') as f:
                    content = f.read()

                # 检查是否有 note-editor 或 note-overlay
                if 'note-editor' in content or 'note-overlay' in content:
                    # 检查 CSS 中是否有 display: none
                    has_editor_hidden = bool(re.search(
                        r'\.note-editor\s*\{[^}]*display:\s*none',
                        content
                    ))

                    has_overlay_hidden = bool(re.search(
                        r'\.note-overlay\s*\{[^}]*display:\s*none',
                        content
                    ))

                    if has_editor_hidden and has_overlay_hidden:
                        print_success(f"{html_file.name}: modal 正确隐藏")
                    else:
                        issues = []
                        if not has_editor_hidden:
                            issues.append("note-editor 没有 display: none")
                        if not has_overlay_hidden:
                            issues.append("note-overlay 没有 display: none")

                        modal_issues[html_file.name] = issues

            except Exception as e:
                self.warnings.append(f"[{html_file.name}] 检查 modal 时出错: {str(e)}")

        for file_name, issues in modal_issues.items():
            for issue in issues:
                self.errors.append(f"[{file_name}] {issue}")

    def check_file_names(self):
        """检查文件名一致性（特别是 QHPhysics vs XunPhysics）"""
        print_info("\n检查文件名一致性...")

        files = list(self.base_path.glob('*.*'))
        file_names = [f.name for f in files]

        # 检查旧的命名
        old_qhphysics = [f for f in file_names if 'QHPhysics' in f]
        if old_qhphysics:
            self.warnings.append(
                f"发现旧命名文件: {', '.join(old_qhphysics)} (应该使用 XunPhysics)"
            )
        else:
            print_success("文件名命名规范（使用 XunPhysics）")

        # 检查对应文件是否成对存在
        html_files = set(f.replace('.html', '') for f in file_names if f.endswith('.html'))
        md_files = set(f.replace('.md', '') for f in file_names if f.endswith('.md'))

        # 特殊处理：某些 HTML 文件加载 markdown，应该检查 fetch 指向
        # 例如 XunPhysics_Architecture.html 应该加载 complete_doc.md
        print_success("文件名一致性检查完成")

    def check_markdown_references(self):
        """检查 HTML 中的 fetch 请求是否指向存在的 markdown 文件"""
        print_info("\n检查 markdown 文件引用...")

        html_files = list(self.base_path.glob('*.html'))

        for html_file in html_files:
            try:
                with open(html_file, 'r', encoding='utf-8') as f:
                    content = f.read()

                # 查找 fetch 请求
                fetches = re.findall(r"fetch\(['\"]([^'\"]+\.md)['\"]", content)

                for md_file in fetches:
                    md_path = self.base_path / md_file

                    if not md_path.exists():
                        self.errors.append(
                            f"[{html_file.name}] fetch 指向不存在的文件: {md_file}"
                        )
                    else:
                        print_success(f"{html_file.name}: 正确加载 {md_file}")

            except Exception as e:
                self.warnings.append(f"[{html_file.name}] 检查 markdown 引用时出错: {str(e)}")

    def print_summary(self):
        """打印总结"""
        print(f"\n{'='*60}")
        print(f"自测结果总结")
        print(f"{'='*60}\n")

        if self.errors:
            print(f"{Colors.RED}发现 {len(self.errors)} 个错误：{Colors.END}")
            for error in self.errors:
                print_error(error)
            print()

        if self.warnings:
            print(f"{Colors.YELLOW}发现 {len(self.warnings)} 个警告：{Colors.END}")
            for warning in self.warnings:
                print_warning(warning)
            print()

        if not self.errors and not self.warnings:
            print_success("所有检查通过！文档质量良好 🎉\n")
            return True

        if self.errors:
            return False

        return True


def main():
    validator = DocValidator()
    success = validator.validate_all()

    # 返回状态码用于 git hook
    sys.exit(0 if success else 1)


if __name__ == '__main__':
    main()
