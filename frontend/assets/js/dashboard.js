import { api } from './api.js';
import { showToast } from './utils.js';
import { requireAuth } from './auth-guard.js';
import { TaskTypeLabels, TaskPriorityLabels } from './constants.js';

document.addEventListener('DOMContentLoaded', async () => {
    // 1. Require authentication for protected route
    requireAuth();

    // DOM Elements - Stat Counters
    const statActiveCourses = document.getElementById('stat-active-courses');
    const statActiveTasks = document.getElementById('stat-active-tasks');
    const statUpcomingExams = document.getElementById('stat-upcoming-exams');

    // DOM Elements - Containers
    const todayClassesContainer = document.getElementById('today-classes-container');
    const upcomingTasksContainer = document.getElementById('upcoming-tasks-container');

    // ==========================================
    // 2. Fetch & Populate Dashboard Summary
    // ==========================================
    async function loadDashboardSummary() {
        try {
            // Show loading state
            if (todayClassesContainer) todayClassesContainer.innerHTML = '<p class="loading-text">در حال دریافت کلاس‌های امروز...</p>';
            if (upcomingTasksContainer) upcomingTasksContainer.innerHTML = '<p class="loading-text">در حال دریافت رویدادهای پیش‌رو...</p>';

            // Fetch summary payload (GET /api/dashboard/summary)
            const summary = await api.get('/dashboard/summary');

            // Update Stat Counters
            if (statActiveCourses) statActiveCourses.textContent = summary.activeCoursesCount || 0;
            if (statActiveTasks) statActiveTasks.textContent = summary.activeTasksCount || 0;
            if (statUpcomingExams) statUpcomingExams.textContent = summary.upcomingExamsCount || 0;

            // Render Today's Classes Timeline
            renderTodayClasses(summary.todayClasses || []);

            // Render Upcoming Tasks & Exams List
            renderUpcomingTasks(summary.upcomingTasks || []);

        } catch (error) {
            console.error('Failed to load dashboard summary:', error);
            showToast('خطا در دریافت اطلاعات داشبورد', 'error');
            
            if (todayClassesContainer) todayClassesContainer.innerHTML = '<p class="error-text">خطا در دریافت اطلاعات.</p>';
            if (upcomingTasksContainer) upcomingTasksContainer.innerHTML = '<p class="error-text">خطا در دریافت اطلاعات.</p>';
        }
    }

    // ==========================================
    // 3. Render Today's Classes List
    // ==========================================
    function renderTodayClasses(classes) {
        if (!todayClassesContainer) return;
        todayClassesContainer.innerHTML = '';

        // Handle Empty State
        if (classes.length === 0) {
            todayClassesContainer.innerHTML = '<p class="empty-message">امروز هیچ کلاسی ندارید. روز آرامی داشته باشید! ☕</p>';
            return;
        }

        classes.forEach(cls => {
            const classCard = document.createElement('div');
            classCard.className = 'class-card';

            const formattedStartTime = cls.startTime ? cls.startTime.slice(0, 5) : '00:00';
            const formattedEndTime = cls.endTime ? cls.endTime.slice(0, 5) : '00:00';

            classCard.innerHTML = `
                <h4 class="course-title">${cls.courseName || 'نامشخص'}</h4>
                <span class="class-time">${formattedStartTime} تا ${formattedEndTime}</span>
                ${cls.location ? `<span class="class-location">${cls.location}</span>` : ''}
            `;

            todayClassesContainer.appendChild(classCard);
        });
    }

    // ==========================================
    // 4. Render Upcoming Tasks & Exams List
    // ==========================================
    function renderUpcomingTasks(tasks) {
        if (!upcomingTasksContainer) return;
        upcomingTasksContainer.innerHTML = '';

        // Handle Empty State
        if (tasks.length === 0) {
            upcomingTasksContainer.innerHTML = '<p class="empty-message">هیچ تکلیف یا امتحانی در روزهای آینده ندارید.</p>';
            return;
        }

        tasks.forEach(task => {
            const taskItem = document.createElement('div');
            taskItem.className = `upcoming-task-card priority-${task.priority}`;

            const formattedDate = new Date(task.dueDate).toLocaleString('fa-IR', {
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });

            taskItem.innerHTML = `
                <div class="task-info">
                    <h4 class="task-title">${task.title}</h4>
                    <span class="course-name">درس: ${task.courseName || 'نامشخص'}</span>
                </div>
                <div class="task-badge-group">
                    <span class="type-badge type-${task.type}">${TaskTypeLabels[task.type] || 'رویداد'}</span>
                    <span class="priority-badge">${TaskPriorityLabels[task.priority] || 'متوسط'}</span>
                    <span class="due-date">${formattedDate}</span>
                </div>
            `;

            upcomingTasksContainer.appendChild(taskItem);
        });
    }

    // Initial Load
    await loadDashboardSummary();
});