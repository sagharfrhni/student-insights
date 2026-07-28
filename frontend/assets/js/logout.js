import { showToast } from './utils.js';

/**
 * Clear user session and redirect to login page
 */
export function logout() {
    // Remove JWT token and user profile from localStorage
    localStorage.removeItem('token');
    localStorage.removeItem('user');

    showToast('شما با موفقیت از حساب کاربری خارج شدید.', 'info');

    setTimeout(() => {
        window.location.href = 'auth.html';
    }, 1000);
}

// Attach event listener to logout button if present on page
document.addEventListener('DOMContentLoaded', () => {
    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', (e) => {
            e.preventDefault();
            logout();
        });
    }
});