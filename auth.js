/**
 * XunPhysics Documentation - Client-side authentication guard
 *
 * Checks whether the current session is authenticated.
 * If not, redirects to the login page.
 *
 * NOTE: This is a lightweight documentation gate, not a security mechanism.
 * The session flag can be set via browser dev-tools. For genuine access control
 * replace this with server-side session validation.
 *
 * Usage: add  <script src="auth.js"></script>  as the FIRST script in <head>.
 */
(function () {
    var SESSION_KEY = 'xunphysics_auth';
    var LOGIN_PAGE  = 'login.html';

    // Determine the root-relative path to login.html (works in any sub-directory)
    function loginUrl() {
        var path = window.location.pathname;
        // Replace the current filename with login.html, keeping the same directory
        var dir = path.substring(0, path.lastIndexOf('/') + 1);
        return dir + LOGIN_PAGE + '?redirect=' + encodeURIComponent(window.location.href);
    }

    if (!sessionStorage.getItem(SESSION_KEY)) {
        window.location.replace(loginUrl());
    }
})();
