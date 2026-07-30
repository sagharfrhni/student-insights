import { api } from './api.js';
import { showToast, openModal, closeModal } from './utils.js';
import { requireAuth } from './auth-guard.js';
import { TaskTypeLabels, TaskPriorityLabels, TaskStatusLabels, TaskStatus } from './constants.js';

document.addEventListener('DOMContentLoaded', async () => {
    // 1. Require authentication for protected route
    requireAuth();

    // DOM Elements - Filters Bar
    const filterTaskCourse = document.getElementById('filter-task-course');
    const filterTaskType = document.getElementById('filter-task-type');
    const filterTaskStatus = document.getElementById('filter-task-status');
    const btnOpenAddTaskModal = document.getElementById('btn-open-add-task-modal');

    // DOM Elements - Tasks List Container
    const tasksListContainer = document.getElementById('tasks-list-container');

    // DOM Elements - Add/Edit Task Modal
    const taskModal = document.getElementById('task-modal');
    const taskForm = document.getElementById('task-form');
    const taskCourseSelect = document.getElementById('task-course-id');
    const taskTitleInput = document.getElementById('task-title');
    const taskDescriptionInput = document.getElementById('task-description');
    const taskTypeSelect = document.getElementById('task-type');
    const taskPrioritySelect = document.getElementById('task-priority');
    const taskDueDateInput = document.getElementById('task-due-date');

    // State variables
    let editingTaskId = null;
    let allCourses = [];
    let currentSemester = null;

    // ==========================================
    // 2. Fetch Initial Dropdown Data (Courses & Active Semester)
    // ==========================================
    async function loadInitialData() {
        try {
            // Fetch semesters to validate dueDate business rule
            const semesters = await api.get('/semesters');
            if (semesters.length > 0) {
                currentSemester = semesters[0]; // Active semester
            }

            // Fetch courses to populate filter and modal dropdowns
            if (currentSemester) {
                allCourses = await api.get(`/courses?semesterId=${currentSemester.id}`);
                populateCourseDropdowns(allCourses);
            }

            // Load initial tasks list
            await loadTasks();

        } catch (error) {
            console.error('Failed to load initial data:', error);
            showToast('خطا در دریافت اطلاعات اولیه', 'error');
        }
    }

    function populateCourseDropdowns(courses) {
        if (!filterTaskCourse || !taskCourseSelect) return;

        // Reset dropdowns
        filterTaskCourse.innerHTML = '<option value="">همه درس‌ها</option>';
        taskCourseSelect.innerHTML = '<option value="">انتخاب درس...</option>';

        courses.forEach(course => {
            const opt1 = document.createElement('option');
            opt1.value = course.id;
            opt1.textContent = course.name;
            filterTaskCourse.appendChild(opt1);

            const opt2 = document.createElement('option');
            opt2.value = course.id;
            opt2.textContent = course.name;
            taskCourseSelect.appendChild(opt2);
        });
    }

    // ==========================================
    // 3. Fetch Tasks List with Optional Filters
    // ==========================================
    async function loadTasks() {
        try {
            tasksListContainer.innerHTML = '<p class="loading-text">در حال دریافت لیست فعالیت‌ها...</p>';

            // Build query parameters based on active filters
            const queryParams = new URLSearchParams();
            if (filterTaskCourse.value) queryParams.append('courseId', filterTaskCourse.value);
            if (filterTaskType.value !== '') queryParams.append('type', filterTaskType.value);
            if (filterTaskStatus.value !== '') queryParams.append('status', filterTaskStatus.value);

            const queryString = queryParams.toString() ? `?${queryParams.toString()}` : '';
            const tasks = await api.get(`/academic-tasks${queryString}`);

            if (tasks.length === 0) {
                tasksListContainer.innerHTML = '<p class="empty-message">هیچ تکلیفی یا امتحانی یافت نشد.</p>';
                return;
            }

            renderTasksList(tasks);

        } catch (error) {
            console.error('Failed to load tasks:', error);
            tasksListContainer.innerHTML = '<p class="error-text">خطا در دریافت لیست فعالیت‌ها.</p>';
        }
    }

    function renderTasksList(tasks) {
        tasksListContainer.innerHTML = '';

        tasks.forEach(task => {
            const taskCard = document.createElement('div');
            taskCard.className = `task-card status-${task.status}`;

            const formattedDate = new Date(task.dueDate).toLocaleString('fa-IR', {
                year: 'numeric',
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });

            const isChecked = task.status === TaskStatus.COMPLETED ? 'checked' : '';

            taskCard.innerHTML = `
                <div class="task-card-header">
                    <div class="task-title-group">
                        <input type="checkbox" 
                               class="task-status-checkbox" 
                               data-action="toggle-task-status" 
                               data-task-id="${task.id}" 
                               ${isChecked}>
                        <h4 class="task-title ${isChecked ? 'completed-text' : ''}">${task.title}</h4>
                    </div>
                    <span class="type-badge type-${task.type}">${TaskTypeLabels[task.type] || 'نامشخص'}</span>
                </div>
                <div class="task-card-body">
                    <p class="course-name">درس: ${task.courseName || 'نامشخص'}</p>
                    ${task.description ? `<p class="task-desc">${task.description}</p>` : ''}
                    <div class="task-meta">
                        <span class="priority-badge priority-${task.priority}">اولویت: ${TaskPriorityLabels[task.priority]}</span>
                        <span class="due-date">مهلت: ${formattedDate}</span>
                    </div>
                </div>
                <div class="task-card-actions">
                    <button class="btn-edit" data-action="edit-task" data-task-id="${task.id}">ویرایش</button>
                    <button class="btn-delete" data-action="delete-task" data-task-id="${task.id}">حذف</button>
                </div>
            `;

            tasksListContainer.appendChild(taskCard);
        });
    }

    // Filter Bar Event Listeners
    if (filterTaskCourse) filterTaskCourse.addEventListener('change', loadTasks);
    if (filterTaskType) filterTaskType.addEventListener('change', loadTasks);
    if (filterTaskStatus) filterTaskStatus.addEventListener('change', loadTasks);

    // ==========================================
    // 4. Quick Status Toggle Handler (PATCH /api/academic-tasks/{id}/status)
    // ==========================================
    tasksListContainer.addEventListener('change', async (e) => {
        const toggleCheckbox = e.target.closest('[data-action="toggle-task-status"]');
        if (toggleCheckbox) {
            const taskId = toggleCheckbox.getAttribute('data-task-id');
            const newStatus = toggleCheckbox.checked ? TaskStatus.COMPLETED : TaskStatus.PENDING;

            try {
                await api.patch(`/academic-tasks/${taskId}/status`, { status: newStatus });
                showToast(newStatus === TaskStatus.COMPLETED ? 'تکلیف انجام شد' : 'تکلیف به جریان افتاد', 'success');
                await loadTasks();
            } catch (error) {
                console.error('Failed to toggle task status:', error);
                toggleCheckbox.checked = !toggleCheckbox.checked; // Revert on failure
            }
        }
    });

    // ==========================================
    // 5. Add / Edit Task Form Submission & Business Rule Validation
    // ==========================================
    if (btnOpenAddTaskModal) {
        btnOpenAddTaskModal.addEventListener('click', () => {
            editingTaskId = null;
            if (taskForm) taskForm.reset();
            openModal('task-modal');
        });
    }

    if (taskForm) {
        taskForm.addEventListener('submit', async (e) => {
            e.preventDefault();

            const courseId = parseInt(taskCourseSelect.value);
            const dueDate = taskDueDateInput.value;

            if (!courseId) {
                showToast('لطفاً یک درس انتخاب کنید', 'warning');
                return;
            }

            // Business Rule 1 Validation: Check if dueDate falls within current semester bounds
            if (currentSemester && dueDate) {
                const selectedDate = new Date(dueDate);
                const semStart = new Date(currentSemester.startDate);
                const semEnd = new Date(currentSemester.endDate);

                if (selectedDate < semStart || selectedDate > semEnd) {
                    showToast('تاریخ تحویل باید حتماً در بازه زمانی ترم مربوطه باشد!', 'warning');
                    return;
                }
            }

            const payload = {
                courseId: courseId,
                title: taskTitleInput.value.trim(),
                description: taskDescriptionInput.value.trim(),
                type: parseInt(taskTypeSelect.value),
                priority: parseInt(taskPrioritySelect.value),
                dueDate: new Date(dueDate).toISOString()
            };

            try {
                if (editingTaskId) {
                    // Update task (PUT /api/academic-tasks/{id})
                    await api.put(`/academic-tasks/${editingTaskId}`, payload);
                    showToast('فعالیت با موفقیت ویرایش شد', 'success');
                } else {
                    // Create new task (POST /api/academic-tasks)
                    await api.post('/academic-tasks', payload);
                    showToast('فعالیت جدید با موفقیت اضافه شد', 'success');
                }

                closeModal('task-modal');
                taskForm.reset();
                editingTaskId = null;
                await loadTasks();

            } catch (error) {
                console.error('Failed to save task:', error);
            }
        });
    }

    // ==========================================
    // 6. Edit & Delete Task Event Delegation
    // ==========================================
    tasksListContainer.addEventListener('click', async (e) => {
        const editBtn = e.target.closest('[data-action="edit-task"]');
        const deleteBtn = e.target.closest('[data-action="delete-task"]');

        if (editBtn) {
            const taskId = editBtn.getAttribute('data-task-id');
            openEditTaskModal(taskId);
        }

        if (deleteBtn) {
            const taskId = deleteBtn.getAttribute('data-task-id');
            if (confirm('آیا از حذف این فعالیت مطمئن هستید؟')) {
                try {
                    await api.delete(`/academic-tasks/${taskId}`);
                    showToast('فعالیت با موفقیت حذف شد', 'success');
                    await loadTasks();
                } catch (error) {
                    console.error('Failed to delete task:', error);
                }
            }
        }
    });

    async function openEditTaskModal(taskId) {
        try {
            // Fetch current tasks list to populate modal fields
            const tasks = await api.get('/academic-tasks');
            const task = tasks.find(t => t.id == taskId);

            if (!task) {
                showToast('اطلاعات فعالیت یافت نشد', 'error');
                return;
            }

            editingTaskId = task.id;
            taskCourseSelect.value = task.courseId;
            taskTitleInput.value = task.title;
            taskDescriptionInput.value = task.description || '';
            taskTypeSelect.value = task.type;
            taskPrioritySelect.value = task.priority;

            // Format date for datetime-local input (YYYY-MM-DDTHH:mm)
            if (task.dueDate) {
                const localDate = new Date(task.dueDate);
                localDate.setMinutes(localDate.getMinutes() - localDate.getTimezoneOffset());
                taskDueDateInput.value = localDate.toISOString().slice(0, 16);
            }

            openModal('task-modal');

        } catch (error) {
            console.error('Failed to fetch task details:', error);
        }
    }

    // Initial Load
    await loadInitialData();
});