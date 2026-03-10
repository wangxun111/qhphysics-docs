/**
 * XunPhysics 文档 - 客户端身份验证守卫
 *
 * 检查当前会话是否已通过身份验证。
 * 若未登录，则跳转到登录页面。
 *
 * 注意：这是一个轻量级文档访问门控，并非安全机制。
 * 会话标志可通过浏览器开发工具手动设置。如需真正的访问控制，
 * 请替换为服务端会话验证。
 *
 * 用法：将 <script src="auth.js"></script> 作为 <head> 中的第一个脚本引入。
 */
(function () {
    var SESSION_KEY = 'xunphysics_auth';
    var LOGIN_PAGE  = 'login.html';

    // 计算 login.html 的根相对路径（适用于任意子目录）
    function loginUrl() {
        var path = window.location.pathname;
        // 将当前文件名替换为 login.html，保持同一目录
        var dir = path.substring(0, path.lastIndexOf('/') + 1);
        return dir + LOGIN_PAGE + '?redirect=' + encodeURIComponent(window.location.href);
    }

    if (!sessionStorage.getItem(SESSION_KEY)) {
        window.location.replace(loginUrl());
    }
})();
