import { api } from './api.js';
import { showToast } from './utils.js';
import { requireAuth } from './auth-guard.js';

document.addEventListener('DOMContentLoaded', async () => {
    // 1. Require authentication for protected route
    requireAuth();

    // DOM Elements
    const adminUsersTableBody = document.getElementById('admin-users-table-body');

    // ==========================================
    // 2. Fetch & Render Users List
    // ==========================================
    async function loadUsersList() {
        try {
            if (adminUsersTableBody) {
                adminUsersTableBody.innerHTML = '<tr><td colspan="5" class="loading-text">در حال دریافت لیست کاربران...</td></tr>';
            }

            // Fetch users list (GET /api/admin/users)
            const users = await api.get('/admin/users');

            if (users.length === 0) {
                adminUsersTableBody.innerHTML = '<tr><td colspan="5" class="empty-message">هیچ کاربری ثبت نشده است.</td></tr>';
                return;
            }

            renderUsersTable(users);

        } catch (error) {
            console.error('Failed to load users:', error);
            showToast('خطا در دریافت لیست کاربران', 'error');
            if (adminUsersTableBody) {
                adminUsersTableBody.innerHTML = '<tr><td colspan="5" class="error-text">خطا در دریافت اطلاعات.</td></tr>';
            }
        }
    }

    function renderUsersTable(users) {
        if (!adminUsersTableBody) return;
        adminUsersTableBody.innerHTML = '';

        users.forEach(user => {
            const tr = document.createElement('tr');
            tr.setAttribute('data-user-id', user.id);

            const statusBadgeHTML = user.isActive
                ? '<span class="badge status-active">فعال</span>'
                : '<span class="badge status-inactive">غیرفعال</span>';

            const buttonText = user.isActive ? 'غیرفعالسازی' : 'فعالسازی';

            tr.innerHTML = `
                <td>${user.fullName || 'نامشخص'}</td>
                <td>${user.email}</td>
                <td>${user.role || 'Student'}</td>
                <td>${statusBadgeHTML}</td>
                <td>
                    <button data-action="toggle-user-status" 
                            data-user-id="${user.id}" 
                            data-is-active="${user.isActive}"
                            class="btn-toggle">
                        ${buttonText}
                    </button>
                </td>
            `;

            adminUsersTableBody.appendChild(tr);
        });
    }

    // ==========================================
    // 3. Toggle User Activation Status Handler
    // ==========================================
    if (adminUsersTableBody) {
        adminUsersTableBody.addEventListener('click', async (e) => {
            const toggleBtn = e.target.closest('[data-action="toggle-user-status"]');
            if (toggleBtn) {
                const userId = toggleBtn.getAttribute('data-user-id');
                const currentStatus = toggleBtn.getAttribute('data-is-active') === 'true';
                const newStatus = !currentStatus;

                try {
                    // Toggle status (PATCH /api/admin/users/{userId}/toggle-status)
                    await api.patch(`/admin/users/${userId}/toggle-status`, { isActive: newStatus });
                    
                    showToast(newStatus ? 'حساب کاربر فعال شد' : 'حساب کاربر غیرفعال شد', 'info');
                    await loadUsersList(); // Refresh table

                } catch (error) {
                    console.error('Failed to toggle user status:', error);
                    showToast('خطا در تغییر وضعیت کاربر', 'error');
                }
            }
        });
    }

    // Initial Load
    await loadUsersList();
});