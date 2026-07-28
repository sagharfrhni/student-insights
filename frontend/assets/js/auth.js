import { api } from './api.js';
import { showToast } from './utils.js';
import { redirectIfAuthenticated } from './auth-guard.js';

document.addEventListener('DOMContentLoaded', () => {
    // 1. Check user session persistence
    // Redirect authenticated users directly to dashboard
    redirectIfAuthenticated();

    // Registration form DOM elements (matching Zahra's DOM contract)
    const registerForm = document.getElementById('register-form');
    const registerFullNameInput = document.getElementById('register-fullname');
    const registerEmailInput = document.getElementById('register-email');
    const registerPasswordInput = document.getElementById('register-password');
    const registerSubmitBtn = document.getElementById('register-submit-btn');
    const registerErrorBox = document.getElementById('register-error-box');

    // Login form DOM elements (matching Zahra's DOM contract)
    const loginForm = document.getElementById('login-form');
    const loginEmailInput = document.getElementById('login-email');
    const loginPasswordInput = document.getElementById('login-password');
    const loginSubmitBtn = document.getElementById('login-submit-btn');
    const loginErrorBox = document.getElementById('login-error-box');

    /**
     * Helper function to render backend validation errors
     * @param {HTMLElement} boxElement
     * @param {Object} errorData
     */
    function renderErrorBox(boxElement, errorData) {
        if (!boxElement) return;

        let htmlContent = '';
        const mainMessage = errorData?.message || 'خطایی در پردازش درخواست رخ داد.';

        htmlContent += `<p class="error-main-title"><strong>${mainMessage}</strong></p>`;

        // Render detailed validation error list if present (Sharif's API contract)
        if (errorData?.errors && Array.isArray(errorData.errors) && errorData.errors.length > 0) {
            htmlContent += '<ul class="error-list">';
            errorData.errors.forEach(err => {
                htmlContent += `<li>${err}</li>`;
            });
            htmlContent += '</ul>';
        }

        boxElement.innerHTML = htmlContent;
        boxElement.classList.remove('hidden'); // Show error box
    }

    /**
     * Helper function to clear and hide error box
     * @param {HTMLElement} boxElement
     */
    function clearErrorBox(boxElement) {
        if (!boxElement) return;
        boxElement.innerHTML = '';
        boxElement.classList.add('hidden'); // Hide error box
    }

    // ==========================================
    // 2. Register Logic (POST /api/auth/register)
    // ==========================================
    if (registerForm) {
        registerForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            clearErrorBox(registerErrorBox);

            const payload = {
                fullName: registerFullNameInput.value.trim(),
                email: registerEmailInput.value.trim(),
                password: registerPasswordInput.value
            };

            // Button loading state
            registerSubmitBtn.classList.add('is-loading');
            registerSubmitBtn.disabled = true;

            try {
                // Send registration request to backend API
                const response = await api.post('/auth/register', payload);

                // Store JWT Token and User info in localStorage
                localStorage.setItem('token', response.token);
                localStorage.setItem('user', JSON.stringify(response.user));

                showToast('ثبت‌نام با موفقیت انجام شد.', 'success');

                // Redirect to dashboard
                setTimeout(() => {
                    window.location.href = 'dashboard.html';
                }, 1000);

            } catch (error) {
                // Render standard validation error response from backend (statusCode 400)
                const errorResponse = error.response?.data;
                renderErrorBox(registerErrorBox, errorResponse);
                showToast('خطا در ثبت‌نام. لطفاً ورودی‌ها را بررسی کنید.', 'error');
            } finally {
                registerSubmitBtn.classList.remove('is-loading');
                registerSubmitBtn.disabled = false;
            }
        });
    }

    // ==========================================
    // 3. Login Logic (POST /api/auth/login)
    // ==========================================
    if (loginForm) {
        loginForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            clearErrorBox(loginErrorBox);

            const payload = {
                email: loginEmailInput.value.trim(),
                password: loginPasswordInput.value
            };

            // Button loading state
            loginSubmitBtn.classList.add('is-loading');
            loginSubmitBtn.disabled = true;

            try {
                // Send login request to backend API
                const response = await api.post('/auth/login', payload);

                // Store JWT Token and User info in localStorage
                localStorage.setItem('token', response.token);
                localStorage.setItem('user', JSON.stringify(response.user));

                showToast(`خوش آمدید، ${response.user.fullName}`, 'success');

                // Redirect to dashboard
                setTimeout(() => {
                    window.location.href = 'dashboard.html';
                }, 1000);

            } catch (error) {
                const errorResponse = error.response?.data;

                // Handle 403 Forbidden (Inactive Account) or 400 Validation Error
                if (error.response?.status === 403) {
                    renderErrorBox(loginErrorBox, { message: 'حساب کاربری شما غیرفعال شده است.' });
                } else {
                    renderErrorBox(loginErrorBox, errorResponse);
                }

                showToast('خطا در ورود به حساب کاربری.', 'error');
            } finally {
                loginSubmitBtn.classList.remove('is-loading');
                loginSubmitBtn.disabled = false;
            }
        });
    }
});