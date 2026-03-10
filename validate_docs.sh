#!/bin/bash
# 文档自测脚本 - 检查常见的文档问题

ERRORS=0
WARNINGS=0

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_error() {
    echo -e "${RED}❌ $1${NC}"
    ((ERRORS++))
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
    ((WARNINGS++))
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

echo ""
echo -e "${BLUE}🔍 开始文档自测...${NC}"
echo ""

# 检查 1: HTML 中的链接是否指向存在的文件
print_info "检查 HTML 文件中的链接..."

for html_file in *.html; do
    [ -f "$html_file" ] || continue

    # 查找所有 href 属性中的本地链接
    grep -o 'href="[^"]*"' "$html_file" | grep -o '"[^"]*"' | tr -d '"' | while read link; do
        # 跳过外部链接和 # 链接
        if [[ "$link" == http* ]] || [[ "$link" == \#* ]]; then
            continue
        fi

        if [ ! -f "$link" ]; then
            print_error "[$html_file] 链接指向不存在的文件: $link"
        else
            print_success "$html_file: 链接正常 $link"
        fi
    done
done

echo ""

# 检查 2: modal 和 overlay 是否正确隐藏
print_info "检查 modal/overlay 是否正确隐藏..."

for html_file in *.html; do
    [ -f "$html_file" ] || continue

    if grep -q "note-editor" "$html_file"; then
        # 检查是否有 display: none (改进的正则，允许空格和换行)
        if grep -E "\.note-editor\s*\{.*display:\s*none" "$html_file" > /dev/null 2>&1 || \
           (grep -A 20 "\.note-editor {" "$html_file" | grep -q "display: none"); then
            print_success "$html_file: note-editor 正确隐藏"
        else
            print_error "[$html_file] .note-editor 缺少 display: none"
        fi
    fi

    if grep -q "note-overlay" "$html_file"; then
        if grep -E "\.note-overlay\s*\{.*display:\s*none" "$html_file" > /dev/null 2>&1 || \
           (grep -A 20 "\.note-overlay {" "$html_file" | grep -q "display: none"); then
            print_success "$html_file: note-overlay 正确隐藏"
        else
            print_error "[$html_file] .note-overlay 缺少 display: none"
        fi
    fi
done

echo ""

# 检查 3: 文件名一致性
print_info "检查文件名一致性..."

OLD_NAMES=$(find . -maxdepth 1 -name "*QHPhysics*" -type f)
if [ -n "$OLD_NAMES" ]; then
    print_warning "发现旧命名文件: $(echo $OLD_NAMES | tr '\n' ' ') (应该使用 XunPhysics)"
else
    print_success "文件名命名规范（使用 XunPhysics）"
fi

echo ""

# 检查 4: markdown 引用检查
print_info "检查 markdown 文件引用..."

for html_file in *.html; do
    [ -f "$html_file" ] || continue

    # 查找所有 fetch 的 markdown 文件
    grep -o "fetch(['\"][^'\"]*\.md['\"]" "$html_file" | grep -o "['\"][^'\"]*['\"]" | tr -d "'" | tr -d '"' | while read md_file; do
        if [ ! -f "$md_file" ]; then
            print_error "[$html_file] fetch 指向不存在的文件: $md_file"
        else
            print_success "$html_file: 正确加载 $md_file"
        fi
    done
done

echo ""
echo "========================================================"
echo "自测结果总结"
echo "========================================================"
echo ""

if [ $ERRORS -gt 0 ]; then
    echo -e "${RED}发现 $ERRORS 个错误${NC}"
fi

if [ $WARNINGS -gt 0 ]; then
    echo -e "${YELLOW}发现 $WARNINGS 个警告${NC}"
fi

if [ $ERRORS -eq 0 ] && [ $WARNINGS -eq 0 ]; then
    echo -e "${GREEN}✅ 所有检查通过！文档质量良好 🎉${NC}"
    echo ""
    exit 0
else
    echo ""
    if [ $ERRORS -gt 0 ]; then
        echo -e "${RED}❌ 存在错误，请修复后重新提交${NC}"
        exit 1
    else
        echo -e "${YELLOW}⚠️  存在警告，但可以继续提交${NC}"
        exit 0
    fi
fi
