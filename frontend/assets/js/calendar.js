import { api } from './api.js';
import { showToast } from './utils.js';
import { requireAuth } from './auth-guard.js';

document.addEventListener('DOMContentLoaded', async () => {
    // 1. Require authentication for protected route
    requireAuth();

    // DOM Elements
    const calPrevBtn = document.getElementById('cal-prev-btn');
    const calNextBtn = document.getElementById('cal-next-btn');
    const calCurrentLabel = document.getElementById('cal-current-label');
    const calendarGridContainer = document.getElementById('calendar-grid-container');

    // State variable for active view date (defaults to current date)
    let currentDate = new Date();

    // ==========================================
    // 2. Month Navigation & Calculation Helpers
    // ==========================================
    function updateMonthLabel(date) {
        if (!calCurrentLabel) return;
        // Display Persian month and year label
        const formattedLabel = date.toLocaleDateString('fa-IR', {
            year: 'numeric',
            month: 'long'
        });
        calCurrentLabel.textContent = formattedLabel;
    }

    if (calPrevBtn) {
        calPrevBtn.addEventListener('click', async () => {
            currentDate.setMonth(currentDate.getMonth() - 1);
            await loadCalendarMonth();
        });
    }

    if (calNextBtn) {
        calNextBtn.addEventListener('click', async () => {
            currentDate.setMonth(currentDate.getMonth() + 1);
            await loadCalendarMonth();
        });
    }

    // ==========================================
    // 3. Fetch Calendar Projection & Render Grid
    // ==========================================
    async function loadCalendarMonth() {
        try {
            updateMonthLabel(currentDate);
            if (calendarGridContainer) {
                calendarGridContainer.innerHTML = '<p class="loading-text">در حال بارگذاری تقویم...</p>';
            }

            // Calculate start and end date for current month in ISO format
            const year = currentDate.getFullYear();
            const month = currentDate.getMonth();

            const startDate = new Date(year, month, 1).toISOString();
            const endDate = new Date(year, month + 1, 0, 23, 59, 59).toISOString();

            // Fetch aggregated events (GET /api/calendar?startDate=&endDate=)
            const events = await api.get(`/calendar?startDate=${startDate}&endDate=${endDate}`);

            // Group events by date string (YYYY-MM-DD)
            const eventsByDate = groupEventsByDate(events);

            // Render monthly calendar grid
            renderCalendarGrid(year, month, eventsByDate);

        } catch (error) {
            console.error('Failed to load calendar data:', error);
            showToast('خطا در دریافت اطلاعات تقویم', 'error');
            if (calendarGridContainer) {
                calendarGridContainer.innerHTML = '<p class="error-text">خطا در دریافت تقویم.</p>';
            }
        }
    }

    function groupEventsByDate(events) {
        const map = {};
        events.forEach(evt => {
            if (evt.date) {
                const dateKey = evt.date.split('T')[0]; // Extract YYYY-MM-DD
                if (!map[dateKey]) map[dateKey] = [];
                map[dateKey].push(evt);
            }
        });
        return map;
    }

    function renderCalendarGrid(year, month, eventsByDate) {
        if (!calendarGridContainer) return;
        calendarGridContainer.innerHTML = '';

        const daysInMonth = new Date(year, month + 1, 0).getDate();

        for (let day = 1; day <= daysInMonth; day++) {
            const dayDate = new Date(year, month, day);
            // Format YYYY-MM-DD for matching events
            const localIsoDate = new Date(dayDate.getTime() - (dayDate.getTimezoneOffset() * 60000))
                .toISOString()
                .split('T')[0];

            const dayCell = document.createElement('div');
            dayCell.className = 'calendar-day';
            dayCell.setAttribute('data-date', localIsoDate);

            // Persian day number
            const persianDayNum = day.toLocaleString('fa-IR');

            // Render events inside this cell
            const dayEvents = eventsByDate[localIsoDate] || [];
            let eventsHTML = '';

            dayEvents.forEach(evt => {
                let badgeClass = 'event-badge ';
                if (evt.itemType === 'Class') badgeClass += 'class-event';
                else if (evt.itemType === 'Exam') badgeClass += 'exam-event priority-high';
                else badgeClass += 'task-event';

                eventsHTML += `<div class="${badgeClass}">${evt.title}</div>`;
            });

            dayCell.innerHTML = `
                <span class="day-number">${persianDayNum}</span>
                <div class="day-events-container">
                    ${eventsHTML}
                </div>
            `;

            calendarGridContainer.appendChild(dayCell);
        }
    }

    // Initial Load
    await loadCalendarMonth();
});