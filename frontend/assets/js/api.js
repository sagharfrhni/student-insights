import { showToast } from './utils.js';

// آدرس اصلی API بک‌اند
const API_BASE_URL = 'http://localhost:5000/api'; 

export const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Request Interceptor
api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Response Interceptor
api.interceptors.response.use(
    (response) => response.data,
    (error) => {
        const status = error.response ? error.response.status : null;
        const message = error.response?.data?.message || 'خطایی در ارتباط با سرور رخ داد.';

        if (status === 401) {
            showToast('نشست شما منقضی شده است. لطفاً مجدداً وارد شوید.', 'error');
            localStorage.removeItem('token');
            setTimeout(() => {
                window.location.href = 'auth.html';
            }, 1500);
        } else if (status === 403) {
            showToast('شما دسترسی به این بخش را ندارید.', 'error');
        } else if (status === 400) {
              showToast(message || 'اطلاعات ورودی نامعتبر است.', 'warning');
        } else {
            showToast(message, 'error');
        }

        return Promise.reject(error);
    }
);