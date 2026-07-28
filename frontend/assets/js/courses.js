import { api } from './api.js';
import { showToast, openModal, closeModal } from './utils.js';
import { requireAuth } from './auth-guard.js';
import { DayOfWeekLabels } from './constants.js';

document.addEventListener('DOMContentLoaded', async () => {
    // 1. Require authentication for protected route
    requireAuth();

    // DOM Elements - Semester Header & Controls
    const semesterSelect = document.getElementById('semester-select');
    const btnOpenAddSemesterModal = document.getElementById('btn-open-add-semester-modal');
    const btnOpenAddCourseModal = document.getElementById('btn-open-add-course-modal');

    // DOM Elements - Courses Grid Container
    const coursesListContainer = document.getElementById('courses-list-container');

    // DOM Elements - Add/Edit Semester Modal
    const semesterModal = document.getElementById('semester-modal');
    const semesterForm = document.getElementById('semester-form');

    // DOM Elements - Add/Edit Course Modal
    const courseModal = document.getElementById('course-modal');
    const courseForm = document.getElementById('course-form');
    const courseNameInput = document.getElementById('course-name');
    const courseTeacherInput = document.getElementById('course-teacher');
    const courseUnitsInput = document.getElementById('course-units');
    const schedulesInputsContainer = document.getElementById('schedules-inputs-container');
    const btnAddScheduleRow = document.getElementById('btn-add-schedule-row');

    // State variables
    let editingCourseId = null;

    // ==========================================
    // 2. Fetch & Render Semesters Dropdown
    // ==========================================
    async function loadSemesters() {
        try {
            const semesters = await api.get('/semesters');
            semesterSelect.innerHTML = '';

            if (semesters.length === 0) {
                semesterSelect.innerHTML = '<option value="">هیچ ترمی یافت نشد</option>';
                coursesListContainer.innerHTML = '<p class="empty-message">لطفاً ابتدا یک ترم جدید اضافه کنید.</p>';
                return;
            }

            semesters.forEach(sem => {
                const option = document.createElement('option');
                option.value = sem.id;
                option.textContent = sem.name;
                semesterSelect.appendChild(option);
            });

            // Trigger loading courses for the first active semester
            loadCoursesBySemester(semesterSelect.value);

        } catch (error) {
            console.error('Failed to load semesters:', error);
            showToast('خطا در دریافت لیست ترم‌ها', 'error');
        }
    }

    // Handle Semester change event
    semesterSelect.addEventListener('change', (e) => {
        const selectedSemesterId = e.target.value;
        if (selectedSemesterId) {
            loadCoursesBySemester(selectedSemesterId);
        }
    });

    // ==========================================
    // 3. Add New Semester Modal Logic
    // ==========================================
    if (btnOpenAddSemesterModal) {
        btnOpenAddSemesterModal.addEventListener('click', () => {
            if (semesterForm) semesterForm.reset();
            openModal('semester-modal');
        });
    }

    if (semesterForm) {
        semesterForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const formData = new FormData(semesterForm);
            const payload = {
                name: formData.get('name'),
                startDate: formData.get('startDate'),
                endDate: formData.get('endDate')
            };

            try {
                await api.post('/semesters', payload);
                showToast('ترم جدید با موفقیت اضافه شد', 'success');
                closeModal('semester-modal');
                semesterForm.reset();
                await loadSemesters(); // Reload dropdown
            } catch (error) {
                console.error('Failed to add semester:', error);
            }
        });
    }

    // ==========================================
    // 4. Fetch & Render Courses Grid
    // ==========================================
    async function loadCoursesBySemester(semesterId) {
        try {
            coursesListContainer.innerHTML = '<p class="loading-text">در حال دریافت لیست دروس...</p>';
            const courses = await api.get(`/courses?semesterId=${semesterId}`);

            if (courses.length === 0) {
                coursesListContainer.innerHTML = '<p class="empty-message">هیچ درسی برای این ترم ثبت نشده است.</p>';
                return;
            }

            renderCoursesList(courses);
        } catch (error) {
            console.error('Failed to load courses:', error);
            coursesListContainer.innerHTML = '<p class="error-text">خطا در دریافت لیست دروس.</p>';
        }
    }

    function renderCoursesList(courses) {
        coursesListContainer.innerHTML = '';

        courses.forEach(course => {
            const courseCard = document.createElement('div');
            courseCard.className = 'course-card';

            // Render schedule badges
            let schedulesHTML = '';
            if (course.schedules && course.schedules.length > 0) {
                schedulesHTML = course.schedules.map(sch => `
                    <div class="schedule-item">
                        <span class="day-badge">${DayOfWeekLabels[sch.dayOfWeek] || 'نامشخص'}</span>
                        <span class="time">${sch.startTime.slice(0, 5)} - ${sch.endTime.slice(0, 5)}</span>
                        ${sch.location ? `<span class="location">${sch.location}</span>` : ''}
                    </div>
                `).join('');
            }

            courseCard.innerHTML = `
                <div class="course-card-header">
                    <h3 class="course-title">${course.name}</h3>
                    <span class="units-badge">${course.units} واحد</span>
                </div>
                <div class="course-card-body">
                    <p class="teacher-name">استاد: ${course.teacherName || 'ثبت نشده'}</p>
                    <div class="course-schedules-list">
                        ${schedulesHTML}
                    </div>
                </div>
                <div class="course-card-actions">
                    <button class="btn-edit" data-action="edit-course" data-course-id="${course.id}">ویرایش</button>
                    <button class="btn-delete" data-action="delete-course" data-course-id="${course.id}">حذف</button>
                </div>
            `;

            coursesListContainer.appendChild(courseCard);
        });
    }

    // ==========================================
    // 5. Dynamic Schedule Row Handling (Add/Remove)
    // ==========================================
    function createScheduleRow(data = {}) {
        const row = document.createElement('div');
        row.className = 'schedule-row';

        row.innerHTML = `
            <select name="dayOfWeek">
                <option value="0" ${data.dayOfWeek == 0 ? 'selected' : ''}>شنبه</option>
                <option value="1" ${data.dayOfWeek == 1 ? 'selected' : ''}>یکشنبه</option>
                <option value="2" ${data.dayOfWeek == 2 ? 'selected' : ''}>دوشنبه</option>
                <option value="3" ${data.dayOfWeek == 3 ? 'selected' : ''}>سه‌شنبه</option>
                <option value="4" ${data.dayOfWeek == 4 ? 'selected' : ''}>چهارشنبه</option>
                <option value="5" ${data.dayOfWeek == 5 ? 'selected' : ''}>پنج‌شنبه</option>
                <option value="6" ${data.dayOfWeek == 6 ? 'selected' : ''}>جمعه</option>
            </select>
            <input type="time" name="startTime" value="${data.startTime ? data.startTime.slice(0, 5) : '08:00'}" required>
            <input type="time" name="endTime" value="${data.endTime ? data.endTime.slice(0, 5) : '10:00'}" required>
            <input type="text" name="location" placeholder="کلاس/مکان" value="${data.location || ''}">
            <button type="button" class="btn-remove-schedule">حذف</button>
        `;

        // Event listener to remove row
        row.querySelector('.btn-remove-schedule').addEventListener('click', () => {
            row.remove();
        });

        schedulesInputsContainer.appendChild(row);
    }

    if (btnAddScheduleRow) {
        btnAddScheduleRow.addEventListener('click', () => {
            createScheduleRow();
        });
    }

    // ==========================================
    // 6. Add/Edit Course Form Submission
    // ==========================================
    if (btnOpenAddCourseModal) {
        btnOpenAddCourseModal.addEventListener('click', () => {
            editingCourseId = null;
            courseForm.reset();
            schedulesInputsContainer.innerHTML = '';
            createScheduleRow(); // Add one default empty schedule row
            openModal('course-modal');
        });
    }

    if (courseForm) {
        courseForm.addEventListener('submit', async (e) => {
            e.preventDefault();

            const activeSemesterId = parseInt(semesterSelect.value);
            if (!activeSemesterId) {
                showToast('لطفاً ابتدا یک ترم انتخاب کنید', 'warning');
                return;
            }

            // Extract schedules from dynamic schedule rows
            const scheduleRows = schedulesInputsContainer.querySelectorAll('.schedule-row');
            const schedules = Array.from(scheduleRows).map(row => ({
                dayOfWeek: parseInt(row.querySelector('[name="dayOfWeek"]').value),
                startTime: row.querySelector('[name="startTime"]').value + ':00',
                endTime: row.querySelector('[name="endTime"]').value + ':00',
                location: row.querySelector('[name="location"]').value.trim()
            }));

            const payload = {
                semesterId: activeSemesterId,
                name: courseNameInput.value.trim(),
                teacherName: courseTeacherInput.value.trim(),
                units: parseInt(courseUnitsInput.value),
                schedules: schedules
            };

            try {
                if (editingCourseId) {
                    // Update existing course (PUT /api/courses/{id})
                    await api.put(`/courses/${editingCourseId}`, payload);
                    showToast('درس با موفقیت ویرایش شد', 'success');
                } else {
                    // Create new course (POST /api/courses)
                    await api.post('/courses', payload);
                    showToast('درس جدید با موفقیت اضافه شد', 'success');
                }

                closeModal('course-modal');
                courseForm.reset();
                editingCourseId = null;
                await loadCoursesBySemester(activeSemesterId);

            } catch (error) {
                console.error('Failed to save course:', error);
            }
        });
    }

    // ==========================================
    // 7. Course Actions Event Delegation (Edit & Delete)
    // ==========================================
    coursesListContainer.addEventListener('click', async (e) => {
        const editBtn = e.target.closest('[data-action="edit-course"]');
        const deleteBtn = e.target.closest('[data-action="delete-course"]');

        if (editBtn) {
            const courseId = editBtn.getAttribute('data-course-id');
            openEditCourseModal(courseId);
        }

        if (deleteBtn) {
            const courseId = deleteBtn.getAttribute('data-course-id');
            if (confirm('آیا از حذف این درس مطمئن هستید؟')) {
                try {
                    await api.delete(`/courses/${courseId}`);
                    showToast('درس با موفقیت حذف شد', 'success');
                    await loadCoursesBySemester(semesterSelect.value);
                } catch (error) {
                    console.error('Failed to delete course:', error);
                }
            }
        }
    });

    async function openEditCourseModal(courseId) {
        try {
            // Fetch current course list to populate modal fields
            const courses = await api.get(`/courses?semesterId=${semesterSelect.value}`);
            const course = courses.find(c => c.id == courseId);

            if (!course) {
                showToast('اطلاعات درس یافت نشد', 'error');
                return;
            }

            editingCourseId = course.id;
            courseNameInput.value = course.name;
            courseTeacherInput.value = course.teacherName;
            courseUnitsInput.value = course.units;

            // Populate schedule rows
            schedulesInputsContainer.innerHTML = '';
            if (course.schedules && course.schedules.length > 0) {
                course.schedules.forEach(sch => createScheduleRow(sch));
            } else {
                createScheduleRow();
            }

            openModal('course-modal');

        } catch (error) {
            console.error('Failed to fetch course details:', error);
        }
    }

    // Initial Load
    await loadSemesters();
});