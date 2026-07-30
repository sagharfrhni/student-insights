import React, { useMemo, useState } from "react";
import "./TaskPage.css";

const mockCourses = [
  { id: 101, name: "ساختمان داده" },
  { id: 102, name: "طراحی پایگاه داده" },
  { id: 103, name: "توسعه وب" },
];

const initialTasks = [
  {
    id: 1,
    courseId: 101,
    courseName: "ساختمان داده",
    title: "تمرین ۱",
    description: "تمرین‌ها و پیاده‌سازی لیست پیوندی.",
    type: 0,
    priority: 2,
    status: 0,
    dueDate: "2026-03-25T23:59:00Z",
  },
  {
    id: 2,
    courseId: 102,
    courseName: "طراحی پایگاه داده",
    title: "امتحان میان‌ترم",
    description: "فصل‌های ۱ تا ۵.",
    type: 1,
    priority: 2,
    status: 0,
    dueDate: "2026-04-10T10:00:00Z",
  },
  {
    id: 3,
    courseId: 103,
    courseName: "توسعه وب",
    title: "گزارش پروژه",
    description: "فایل نهایی PDF را ارسال کنید.",
    type: 0,
    priority: 1,
    status: 1,
    dueDate: "2026-03-20T18:00:00Z",
  },
];

const typeLabel = {
  0: "تکلیف",
  1: "امتحان",
};

const priorityLabel = {
  0: "کم",
  1: "متوسط",
  2: "زیاد",
};

const statusLabel = {
  0: "در انتظار",
  1: "تکمیل‌شده",
  2: "لغوشده",
};

function formatDateTime(value) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return date.toLocaleString("fa-IR");
}

export default function TaskPage() {
  const [tasks, setTasks] = useState(initialTasks);
  const [selectedStatus, setSelectedStatus] = useState("all");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingTaskId, setEditingTaskId] = useState(null);

  const [formData, setFormData] = useState({
    courseId: String(mockCourses[0]?.id ?? ""),
    title: "",
    description: "",
    type: "0",
    priority: "1",
    dueDate: "",
  });

  const filteredTasks = useMemo(() => {
    return tasks.filter((task) => {
      if (selectedStatus === "all") return true;
      return String(task.status) === selectedStatus;
    });
  }, [tasks, selectedStatus]);

  const openAddModal = () => {
    setEditingTaskId(null);
    setFormData({
      courseId: String(mockCourses[0]?.id ?? ""),
      title: "",
      description: "",
      type: "0",
      priority: "1",
      dueDate: "",
    });
    setIsModalOpen(true);
  };

  const openEditModal = (task) => {
    setEditingTaskId(task.id);
    setFormData({
      courseId: String(task.courseId),
      title: task.title ?? "",
      description: task.description ?? "",
      type: String(task.type ?? 0),
      priority: String(task.priority ?? 1),
      dueDate: task.dueDate ? task.dueDate.slice(0, 16) : "",
    });
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setEditingTaskId(null);
  };

  const handleChange = (event) => {
    const { name, value } = event.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = (event) => {
    event.preventDefault();

    const course = mockCourses.find((item) => item.id === Number(formData.courseId));

    const payload = {
      id: editingTaskId ?? Date.now(),
      courseId: Number(formData.courseId),
      courseName: course?.name ?? "درس نامشخص",
      title: formData.title.trim(),
      description: formData.description.trim(),
      type: Number(formData.type),
      priority: Number(formData.priority),
      status: editingTaskId
        ? tasks.find((task) => task.id === editingTaskId)?.status ?? 0
        : 0,
      dueDate: new Date(formData.dueDate).toISOString(),
    };

    if (editingTaskId) {
      setTasks((prev) => prev.map((task) => (task.id === editingTaskId ? payload : task)));
    } else {
      setTasks((prev) => [payload, ...prev]);
    }

    closeModal();
  };

  const toggleTaskStatus = (taskId) => {
    setTasks((prev) =>
      prev.map((task) =>
        task.id === taskId
          ? { ...task, status: task.status === 1 ? 0 : 1 }
          : task
      )
    );
  };

  const deleteTask = (taskId) => {
    const confirmed = window.confirm("آیا از حذف این وظیفه مطمئن هستید؟");
    if (!confirmed) return;
    setTasks((prev) => prev.filter((task) => task.id !== taskId));
  };

  return (
    <div className="task-page">
      <header className="task-page__header">
        <div>
          <h1 className="task-page__title">وظایف آموزشی</h1>
          <p className="task-page__subtitle">
            تکالیف و امتحان‌ها را در یک مکان مدیریت کنید.
          </p>
        </div>

        <button
          id="btn-open-add-task-modal"
          type="button"
          className="btn btn--primary"
          onClick={openAddModal}
        >
          + افزودن وظیفه
        </button>
      </header>

      <section className="task-page__filters">
        <label className="filter-group" htmlFor="filter-task-status">
          <span className="filter-group__label">وضعیت</span>
          <select
            id="filter-task-status"
            className="filter-group__select"
            value={selectedStatus}
            onChange={(e) => setSelectedStatus(e.target.value)}
          >
            <option value="all">همه</option>
            <option value="0">در انتظار</option>
            <option value="1">تکمیل‌شده</option>
            <option value="2">لغوشده</option>
          </select>
        </label>
      </section>

      <main id="task-list-container" className="task-list">
        {filteredTasks.length === 0 ? (
          <div className="empty-state">
            <h2>وظیفه‌ای پیدا نشد</h2>
            <p>فیلتر را تغییر دهید یا یک وظیفه جدید اضافه کنید.</p>
          </div>
        ) : (
          filteredTasks.map((task) => (
            <article key={task.id} className="task-card">
              <div className="task-card__main">
                <div className="task-card__top">
                  <div>
                    <h3 className="task-card__title">{task.title}</h3>
                    <p className="task-card__course">{task.courseName}</p>
                  </div>

                  <span className={`badge badge--type-${task.type}`}>
                    {typeLabel[task.type]}
                  </span>
                </div>

                <p className="task-card__description">{task.description}</p>

                <div className="task-card__meta">
                  <span>
                    <strong>اولویت:</strong> {priorityLabel[task.priority]}
                  </span>
                  <span>
                    <strong>وضعیت:</strong> {statusLabel[task.status]}
                  </span>
                  <span>
                    <strong>مهلت:</strong> {formatDateTime(task.dueDate)}
                  </span>
                </div>
              </div>

              <div className="task-card__actions">
                <label className="status-toggle">
                  <input
                    type="checkbox"
                    data-action="toggle-task-status"
                    data-task-id={task.id}
                    checked={task.status === 1}
                    onChange={() => toggleTaskStatus(task.id)}
                  />
                  <span>تکمیل‌شده</span>
                </label>

                <button
                  type="button"
                  className="btn btn--secondary"
                  data-action="edit-task"
                  data-task-id={task.id}
                  onClick={() => openEditModal(task)}
                >
                  ویرایش
                </button>

                <button
                  type="button"
                  className="btn btn--danger"
                  data-action="delete-task"
                  data-task-id={task.id}
                  onClick={() => deleteTask(task.id)}
                >
                  حذف
                </button>
              </div>
            </article>
          ))
        )}
      </main>

      {isModalOpen && (
        <div id="task-modal" className="modal" onClick={closeModal}>
          <div className="modal__content" onClick={(e) => e.stopPropagation()}>
            <div className="modal__header">
              <h2>{editingTaskId ? "ویرایش وظیفه" : "افزودن وظیفه"}</h2>
              <button
                type="button"
                className="modal__close"
                onClick={closeModal}
                aria-label="بستن پنجره"
              >
                ×
              </button>
            </div>

            <form id="task-form" className="task-form" onSubmit={handleSubmit}>
              <label className="form-field">
                <span>درس</span>
                <select
                  id="task-course-id"
                  name="courseId"
                  value={formData.courseId}
                  onChange={handleChange}
                  required
                >
                  {mockCourses.map((course) => (
                    <option key={course.id} value={course.id}>
                      {course.name}
                    </option>
                  ))}
                </select>
              </label>

              <label className="form-field">
                <span>عنوان</span>
                <input
                  id="task-title"
                  type="text"
                  name="title"
                  value={formData.title}
                  onChange={handleChange}
                  required
                  maxLength={150}
                />
              </label>

              <label className="form-field">
                <span>توضیحات</span>
                <textarea
                  id="task-description"
                  name="description"
                  value={formData.description}
                  onChange={handleChange}
                  rows={4}
                  maxLength={1000}
                />
              </label>

              <label className="form-field">
                <span>نوع</span>
                <select
                  id="task-type"
                  name="type"
                  value={formData.type}
                  onChange={handleChange}
                  required
                >
                  <option value="0">تکلیف</option>
                  <option value="1">امتحان</option>
                </select>
              </label>

              <label className="form-field">
                <span>اولویت</span>
                <select
                  id="task-priority"
                  name="priority"
                  value={formData.priority}
                  onChange={handleChange}
                  required
                >
                  <option value="0">کم</option>
                  <option value="1">متوسط</option>
                  <option value="2">زیاد</option>
                </select>
              </label>

              <label className="form-field">
                <span>تاریخ سررسید</span>
                <input
                  id="task-due-date"
                  type="datetime-local"
                  name="dueDate"
                  value={formData.dueDate}
                  onChange={handleChange}
                  required
                />
              </label>

              <div className="task-form__actions">
                <button type="button" className="btn btn--ghost" onClick={closeModal}>
                  انصراف
                </button>
                <button type="submit" className="btn btn--primary">
                  {editingTaskId ? "ذخیره تغییرات" : "ایجاد وظیفه"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
