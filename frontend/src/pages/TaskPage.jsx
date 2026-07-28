// TaskPage.jsx
import React, { useMemo, useState } from "react";
import "./TaskPage.css";

const mockCourses = [
  { id: 101, name: "Data Structures" },
  { id: 102, name: "Database Design" },
  { id: 103, name: "Web Development" },
];

const initialTasks = [
  {
    id: 1,
    courseId: 101,
    courseName: "Data Structures",
    title: "Homework 1",
    description: "Linked list exercises and implementation.",
    type: 0,
    priority: 2,
    status: 0,
    dueDate: "2026-03-25T23:59:00Z",
  },
  {
    id: 2,
    courseId: 102,
    courseName: "Database Design",
    title: "Midterm Exam",
    description: "Chapters 1 to 5.",
    type: 1,
    priority: 2,
    status: 0,
    dueDate: "2026-04-10T10:00:00Z",
  },
  {
    id: 3,
    courseId: 103,
    courseName: "Web Development",
    title: "Project Report",
    description: "Submit the final PDF report.",
    type: 0,
    priority: 1,
    status: 1,
    dueDate: "2026-03-20T18:00:00Z",
  },
];

const typeLabel = {
  0: "Assignment",
  1: "Exam",
};

const priorityLabel = {
  0: "Low",
  1: "Medium",
  2: "High",
};

const statusLabel = {
  0: "Pending",
  1: "Completed",
  2: "Cancelled",
};

function formatDateTime(value) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return date.toLocaleString("en-US");
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
      courseName: course?.name ?? "Unknown Course",
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
    const confirmed = window.confirm("Are you sure you want to delete this task?");
    if (!confirmed) return;
    setTasks((prev) => prev.filter((task) => task.id !== taskId));
  };

  return (
    <div className="task-page">
      <header className="task-page__header">
        <div>
          <h1 className="task-page__title">Academic Tasks</h1>
          <p className="task-page__subtitle">
            Manage assignments and exams in one place.
          </p>
        </div>

        <button
          id="btn-open-add-task-modal"
          type="button"
          className="btn btn--primary"
          onClick={openAddModal}
        >
          + Add Task
        </button>
      </header>

      <section className="task-page__filters">
        <label className="filter-group" htmlFor="filter-task-status">
          <span className="filter-group__label">Status</span>
          <select
            id="filter-task-status"
            className="filter-group__select"
            value={selectedStatus}
            onChange={(e) => setSelectedStatus(e.target.value)}
          >
            <option value="all">All</option>
            <option value="0">Pending</option>
            <option value="1">Completed</option>
            <option value="2">Cancelled</option>
          </select>
        </label>
      </section>

      <main id="task-list-container" className="task-list">
        {filteredTasks.length === 0 ? (
          <div className="empty-state">
            <h2>No tasks found</h2>
            <p>Try changing the filter or add a new task.</p>
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
                    <strong>Priority:</strong> {priorityLabel[task.priority]}
                  </span>
                  <span>
                    <strong>Status:</strong> {statusLabel[task.status]}
                  </span>
                  <span>
                    <strong>Due:</strong> {formatDateTime(task.dueDate)}
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
                  <span>Completed</span>
                </label>

                <button
                  type="button"
                  className="btn btn--secondary"
                  data-action="edit-task"
                  data-task-id={task.id}
                  onClick={() => openEditModal(task)}
                >
                  Edit
                </button>

                <button
                  type="button"
                  className="btn btn--danger"
                  data-action="delete-task"
                  data-task-id={task.id}
                  onClick={() => deleteTask(task.id)}
                >
                  Delete
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
              <h2>{editingTaskId ? "Edit Task" : "Add Task"}</h2>
              <button
                type="button"
                className="modal__close"
                onClick={closeModal}
                aria-label="Close modal"
              >
                ×
              </button>
            </div>

            <form id="task-form" className="task-form" onSubmit={handleSubmit}>
              <label className="form-field">
                <span>Course</span>
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
                <span>Title</span>
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
                <span>Description</span>
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
                <span>Type</span>
                <select
                  id="task-type"
                  name="type"
                  value={formData.type}
                  onChange={handleChange}
                  required
                >
                  <option value="0">Assignment</option>
                  <option value="1">Exam</option>
                </select>
              </label>

              <label className="form-field">
                <span>Priority</span>
                <select
                  id="task-priority"
                  name="priority"
                  value={formData.priority}
                  onChange={handleChange}
                  required
                >
                  <option value="0">Low</option>
                  <option value="1">Medium</option>
                  <option value="2">High</option>
                </select>
              </label>

              <label className="form-field">
                <span>Due Date</span>
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
                  Cancel
                </button>
                <button type="submit" className="btn btn--primary">
                  {editingTaskId ? "Save Changes" : "Create Task"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
